// Copyright 2013, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)
// Author: Chris Seeley (https://github.com/Narwalter)

using System;
using System.Collections.Generic;
using System.Text;
using Google.Api.Ads.Dfp.v201211;
using DateTime = Google.Api.Ads.Dfp.v201211.DateTime;

namespace Google.Api.Ads.Dfp.Util.v201211 {

  /// <summary>
  /// A utility class that allows for statements to be constructed in parts.
  /// Typical usage is:
  /// <code>
  /// StatementBuilder statementBuilder = new StatementBuilder()
  ///     .Where("lastModifiedTime > :yesterday AND type = :type")
  ///     .OrderBy("name DESC")
  ///     .Limit(200)
  ///     .Offset(20)
  ///     .AddValue("yesterday",
  ///         DateTimeUtilities.FromDateTime(System.DateTime.Now.AddDays(-1)))
  ///     .AddValue("type", "Type");
  /// Statement statement = statementBuilder.ToStatement();
  /// // ...
  /// statementBuilder.increaseOffsetBy(20);
  /// statement = statementBuilder.ToStatement();
  /// </code>
  /// </summary>
  public class StatementBuilder {
    public const int SUGGESTED_PAGE_LIMIT = 500;

    private const string SELECT = "SELECT";
    private const string FROM = "FROM";
    private const string WHERE = "WHERE";
    private const string LIMIT = "LIMIT";
    private const string OFFSET = "OFFSET";
    private const string ORDER_BY = "ORDER BY";

    protected string query;
    protected string select;
    protected string from;
    protected string where;
    protected int? limit = null;
    protected int? offset = null;
    protected string orderBy;

    /// <summary>
    /// The list of query parameters.
    /// </summary>
    private List<String_ValueMapEntry> valueEntries;

    /// <summary>
    /// Constructs a statement builder for partial query building.
    /// </summary>
    public StatementBuilder() {
      valueEntries = new List<String_ValueMapEntry>();
    }

    /// <summary>
    /// Constructs a statement builder with a complete query.
    /// </summary>
    /// <param name="query">The query string.</param>
    [Obsolete("Use partial query construction instead")]
    public StatementBuilder(string query) {
      this.query = query;
      valueEntries = new List<String_ValueMapEntry>();
    }

    /// <summary>
    /// Gets or sets the statement query.
    /// </summary>
    [Obsolete("Use partial query construction instead")]
    public string Query {
      get {
        ValidateUsingCompleteQuery();
        return query;
      }
      set {
        ValidateUsingCompleteQuery();
        query = value;
      }
    }

    /// <summary>
    /// Removes a keyword from the start of a clause, if it exists.
    /// </summary>
    /// <param name="clause">The clause to remove the keyword from</param>
    /// <param name="keyword">The keyword to remove</param>
    /// <returns>The clause with the keyword removed</returns>
    private static string RemoveKeyword(string clause, string keyword) {
      string formattedKeyword = keyword.Trim() + " ";
      return clause.StartsWith(formattedKeyword, true, null)
          ? clause.Substring(formattedKeyword.Length) : clause;
    }

    private void ValidateUsingCompleteQuery() {
      if (PartialQueryExists()) {
        throw new InvalidOperationException(DfpErrorMessages.PartialQuerySet);
      }
    }

    private bool PartialQueryExists() {
      return select != null
        || from != null
        || where != null
        || limit != null
        || offset != null
        || orderBy != null;
    }

    private void ValidateUsingPartialQuery() {
      if (!String.IsNullOrEmpty(query)) {
        throw new InvalidOperationException(DfpErrorMessages.CompleteQuerySet);
      }
    }

    /// <summary>
    /// Sets the statement SELECT clause in the form of "a,b".
    /// Only necessary for statements being sent to the
    /// <see cref="PublisherQueryLanguageService"/>.
    /// The "SELECT " keyword will be ignored.
    /// </summary>
    /// <param name="columns">
    /// The statement serlect clause without "SELECT".
    /// </param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder Select(String columns) {
      PreconditionUtilities.CheckArgumentNotNull(columns, "columns");
      ValidateUsingPartialQuery();
      this.select = RemoveKeyword(columns, SELECT);
      return this;
    }

    /// <summary>
    /// Sets the statement FROM clause in the form of "table".
    /// Only necessary for statements being sent to the
    /// <see cref="PublisherQueryLanguageService"/>.
    /// The "FROM " keyword will be ignored.
    /// </summary>
    /// <param name="table">The statement from clause without "FROM"</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder From(String table) {
      PreconditionUtilities.CheckArgumentNotNull(table, "table");
      ValidateUsingPartialQuery();
      this.from = RemoveKeyword(table, FROM);
      return this;
    }

    /// <summary>
    /// Sets the statement WHERE clause in the form of
    /// <code>
    /// "WHERE &lt;condition&gt; {[AND | OR] &lt;condition&gt; ...}"
    /// </code>
    /// e.g. "a = b OR b = c". The "WHERE " keyword will be ignored.
    /// </summary>
    /// <param name="conditions">The statement query without "WHERE"</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder Where(String conditions) {
      PreconditionUtilities.CheckArgumentNotNull(conditions, "conditions");
      ValidateUsingPartialQuery();
      this.where = RemoveKeyword(conditions, WHERE);
      return this;
    }

    /// <summary>
    /// Sets the statement LIMIT clause in the form of
    /// <code>"LIMIT &lt;count&gt;"</code>
    /// </summary>
    /// <param name="count">the statement limit</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder Limit(Int32 count) {
      ValidateUsingPartialQuery();
      this.limit = count;
      return this;
    }

    /// <summary>
    /// Sets the statement OFFSET clause in the form of
    /// <code>"OFFSET &lt;count&gt;"</code>
    /// </summary>
    /// <param name="count">the statement offset</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder Offset(Int32 count) {
      ValidateUsingPartialQuery();
      this.offset = count;
      return this;
    }

    /// <summary>
    /// Increases the offset by the given amount.
    /// </summary>
    /// <param name="amount">the amount to increase the offset</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder IncreaseOffsetBy(Int32 amount) {
      ValidateUsingPartialQuery();
      if (this.offset == null) {
        this.offset = 0;
      }
      this.offset += amount;
      return this;
    }

    /// <summary>
    /// Gets the curent offset
    /// </summary>
    /// <returns>The current offset</returns>
    public int? GetOffset() {
      ValidateUsingPartialQuery();
      return this.offset;
    }

    /// <summary>
    /// Removes the limit and offset from the query.
    /// </summary>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder RemoveLimitAndOffset() {
      ValidateUsingPartialQuery();
      this.offset = null;
      this.limit = null;
      return this;
    }

    /// <summary>
    /// Sets the statement ORDER BY clause in the form of
    /// <code>"ORDER BY &lt;property&gt; [ASC | DESC]"</code>
    /// e.g. "type ASC, lastModifiedDateTime DESC".
    /// The "ORDER BY " keyword will be ignored.
    /// </summary>
    /// <param name="orderBy">the statement order by without "ORDER BY"</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder OrderBy(String orderBy) {
      PreconditionUtilities.CheckArgumentNotNull(orderBy, "orderBy");
      this.orderBy = RemoveKeyword(orderBy, ORDER_BY);
      return this;
    }

    /// <summary>
    /// Adds a new string value to the list of query parameters.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder AddValue(string key, string value) {
      TextValue queryValue = new TextValue();
      queryValue.value = value;
      return AddValue(key, queryValue);
    }

    /// <summary>
    /// Adds a new boolean value to the list of query parameters.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder AddValue(string key, bool value) {
      BooleanValue queryValue = new BooleanValue();
      queryValue.value = value;
      return AddValue(key, queryValue);
    }

    /// <summary>
    /// Adds a new decimal value to the list of query parameters.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder AddValue(string key, decimal value) {
      NumberValue queryValue = new NumberValue();
      queryValue.value = value.ToString();
      return AddValue(key, queryValue);
    }

    /// <summary>
    /// Adds a new DateTime value to the list of query parameters.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    public StatementBuilder AddValue(string key, DateTime value) {
      DateTimeValue queryValue = new DateTimeValue();
      queryValue.value = value;
      return AddValue(key, queryValue);
    }

    /// <summary>
    /// Adds a new value to the list of query parameters.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The statement builder, for chaining method calls.</returns>
    private StatementBuilder AddValue(string key, Value value) {
      String_ValueMapEntry queryValue = new String_ValueMapEntry();
      queryValue.key = key;
      queryValue.value = value;
      valueEntries.Add(queryValue);
      return this;
    }

    private void ValidateQuery() {
      if (limit == null && offset != null) {
        throw new InvalidOperationException(
          DfpErrorMessages.InvalidOffsetAndLimit);
      }
    }

    private String BuildQuery() {
      ValidateQuery();
      StringBuilder stringBuilder = new StringBuilder();
      if (!String.IsNullOrEmpty(select)) {
        stringBuilder = stringBuilder.Append(SELECT).Append(" ")
          .Append(select).Append(" ");
      }
      if (!String.IsNullOrEmpty(from)) {
        stringBuilder = stringBuilder.Append(FROM).Append(" ")
          .Append(from).Append(" ");
      }
      if (!String.IsNullOrEmpty(where)) {
        stringBuilder = stringBuilder.Append(WHERE).Append(" ")
          .Append(where).Append(" ");
      }
      if (!String.IsNullOrEmpty(orderBy)) {
        stringBuilder = stringBuilder.Append(ORDER_BY).Append(" ")
          .Append(orderBy).Append(" ");
      }
      if (limit != null) {
        stringBuilder = stringBuilder.Append(LIMIT).Append(" ")
          .Append(limit).Append(" ");
      }
      if (offset != null) {
        stringBuilder = stringBuilder.Append(OFFSET).Append(" ")
          .Append(offset).Append(" ");
      }
      return stringBuilder.ToString().Trim();
    }

    /// <summary>
    /// Gets the <see cref="Statement"/> representing the state of this
    /// statement builder.
    /// </summary>
    /// <returns>The statement.</returns>
    public Statement ToStatement() {
      Statement statement = new Statement();
      statement.query = !String.IsNullOrEmpty(query) ? query : BuildQuery();
      statement.values = valueEntries.ToArray();
      return statement;
    }
  }
}
