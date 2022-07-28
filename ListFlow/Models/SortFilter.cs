using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ListFlow.Models
{
    public class SortFilter : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<string> filterLogics;
        private ObservableCollection<string> filterFields;
        private ObservableCollection<string> filterComparisons;
        private ObservableCollection<string> filterComparesTo;
        private ObservableCollection<bool> filterHasValue;

        private ObservableCollection<string> sortFields;
        private ObservableCollection<bool> sortDirections;

        #endregion

        #region Properties

        public ObservableCollection<string> FilterComparisons
        {
            get => filterComparisons;
            set
            {
                if (filterComparisons != value)
                {
                    filterComparisons = value;
                    OnPropertyChanged(nameof(FilterComparisons));
                }
            }
        }
        public ObservableCollection<string> FilterLogics
        {
            get => filterLogics;
            set
            {
                if (filterLogics != value)
                {
                    filterLogics = value;
                    OnPropertyChanged(nameof(FilterLogics));
                }
            }
        }
        public ObservableCollection<string> FilterFields
        {
            get => filterFields;
            set
            {
                if (filterFields != value)
                {
                    filterFields = value;
                    OnPropertyChanged(nameof(FilterFields));
                }
            }
        }
        public ObservableCollection<string> FilterComparesTo
        {
            get => filterComparesTo;
            set
            {
                if (filterComparesTo != value)
                {
                    filterComparesTo = value;
                    OnPropertyChanged(nameof(FilterComparesTo));
                }
            }
        }
        public ObservableCollection<bool> FilterHasValue => filterHasValue;

        public ObservableCollection<string> SortFields
        {
            get => sortFields;
            set
            {
                if (sortFields != value)
                {
                    sortFields = value;
                    OnPropertyChanged(nameof(SortFields));
                }
            }
        }
        public ObservableCollection<bool> SortDirections
        {
            get => sortDirections;
            set
            {
                if (sortDirections != value)
                {
                    sortDirections = value;
                    OnPropertyChanged(nameof(SortDirections));
                }
            }
        }

        public Dictionary<string, string> Logics { get; set; }
        public Dictionary<string, string> Comparisons { get; set; }

        #endregion

        #region Constructors

        public SortFilter(string sql)
        {
            FilterComparisons = new ObservableCollection<string>(new string[8].ToList());
            FilterLogics = new ObservableCollection<string>(new string[8].ToList());
            FilterFields = new ObservableCollection<string>(new string[8].ToList());
            FilterComparesTo = new ObservableCollection<string>(new string[8].ToList());
            filterHasValue = new ObservableCollection<bool>(new bool[8].ToList());

            SortFields = new ObservableCollection<string>(new string[8].ToList());
            SortDirections = new ObservableCollection<bool>(new bool[8].ToList());

            //ResetFilter();
            //ResetSort();

            FillLists();

            _ = ParseSQL(sql);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reset user defined sorts.
        /// </summary>
        public void ResetSort()
        {
            for (int i = 0; i < SortFields.Count; i++)
            {
                SortFields[i] = string.Empty;
                SortDirections[i] = true;
            }
        }

        /// <summary>
        /// Reset user defined filters.
        /// </summary>
        public void ResetFilter()
        {
            for (int i = 0; i < FilterFields.Count; i++)
            {
                FilterLogics[i] = string.Empty;
                FilterFields[i] = string.Empty;
                FilterComparisons[i] = string.Empty;
                FilterComparesTo[i] = string.Empty;
                FilterHasValue[i] = false;
            }
        }

        /// <summary>
        /// Fill the sorts and filters comparaisons lists.
        /// </summary>
        private void FillLists()
        {
            // Fill Sort criteria list.
            Logics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AND", Properties.Resources.Filter_And },
                { "OR", Properties.Resources.Filter_Or }
            };

            // Fill Comparisons criteria list.
            Comparisons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "=", Properties.Resources.Filter_Comparison_Eq },
                { "<>",  Properties.Resources.Filter_Comparison_Neq },
                { "<", Properties.Resources.Filter_Comparison_Lt },
                { ">", Properties.Resources.Filter_Comparison_Gt },
                { "<=", Properties.Resources.Filter_Comparison_Lte },
                { ">=", Properties.Resources.Filter_Comparison_Gte },
                { "IS NULL", Properties.Resources.Filter_Comparison_Blk },
                { "IS NOT NULL", Properties.Resources.Filter_Comparison_Nblk },
                { "LIKE", Properties.Resources.Filter_Comparison_Contains },
                { "NOT LIKE", Properties.Resources.Filter_Comparison_NotContains }
            };
        }

        /// <summary>
        /// Builds the SQL code according to the parameters defined by the user.
        /// </summary>
        /// <param name="sheetName">Name of the Sheet in Excel.</param>
        /// <param name="fieldContentTypes">List of fields with their data types.</param>
        /// <returns>SQL code.</returns>
        public string BuildSQL(string sheetName, Dictionary<string, Type> fieldContentTypes)
        {
            StringBuilder sql = new StringBuilder($"SELECT * FROM `{sheetName}$` ");

            // Filters (WHERE SQL part).
            if (filterFields.Any(x => !string.IsNullOrEmpty(x)))
            {
                _ = sql.Append("WHERE ");

                for (int i = 0; i < filterFields.Count; i++)
                {
                    if (!string.IsNullOrEmpty(filterFields[i]))
                    {
                        switch (filterComparisons[i])
                        {
                            case "<>":
                            case "<":
                            case "<=":
                            case "=":
                            case ">":
                            case ">=":
                                if (fieldContentTypes[filterFields[i]] == typeof(double))
                                {
                                    _ = sql.Append($"{filterLogics[i]} [{filterFields[i]}]{filterComparisons[i]}{filterComparesTo[i].Trim()} ".TrimStart());
                                }
                                else
                                { 
                                    _ = sql.Append($"{filterLogics[i]} [{filterFields[i]}] {filterComparisons[i]} '{filterComparesTo[i].Trim()}' ".TrimStart());
                                }
                                break;
                            case "IS NULL":
                                if (fieldContentTypes[filterFields[i]] == typeof(double))
                                {
                                    _ = sql.Append($"{filterLogics[i]} [{filterFields[i]}] {filterComparisons[i]} ".TrimStart());
                                }
                                else
                                {
                                    _ = sql.Append($"{filterLogics[i]} ([{filterFields[i]}] {filterComparisons[i]} OR [{filterFields[i]}]='') ".TrimStart());
                                }
                                break;
                            case "IS NOT NULL":
                                if (fieldContentTypes[filterFields[i]] == typeof(double))
                                {
                                    _ = sql.Append($"{filterLogics[i]} [{filterFields[i]}] {filterComparisons[i]} ".TrimStart());
                                }
                                else
                                {
                                    _ = sql.Append($"{filterLogics[i]} ([{filterFields[i]}] {filterComparisons[i]} AND [{filterFields[i]}]<>'') ".TrimStart());
                                }                                
                                break;
                            case "LIKE":
                            case "NOT LIKE":
                                _ = sql.Append($"{filterLogics[i]} [{filterFields[i]}] {filterComparisons[i]} '%{filterComparesTo[i].Trim()}%' ".TrimStart());
                                break;
                        }
                    }
                }
            }

            // Sort (ORDER BY SQL part).
            if (sortFields.Any(x => !string.IsNullOrEmpty(x)))
            {
                _ = sql.Append("ORDER BY ");

                string sortDir;

                for (int i = 0; i < sortFields.Count; i++)
                {
                    if (!string.IsNullOrEmpty(sortFields[i]))
                    {
                        sortDir = sortDirections[i] ? "ASC" : "DESC";
                        _ = sql.Append($"`{sortFields[i]}` {sortDir},");
                    }
                }

                // Remove the last comma.
                _ = sql.Remove(sql.Length - 1, 1);
            }

            return sql.ToString().Trim();
        }

        public bool FlattenSQL(string sql)
        {
            bool success = false;

            // SQL Parser.
            TSql150Parser parser = new TSql150Parser(true, SqlEngineType.All);

            _ = parser.Parse(new StringReader(sql), out IList<ParseError> parseErrors);

            if (parseErrors.Count == 0)
            {
                // Create the list of tokens (parse sql).
                List<TokenInfo> tokens = ParseSql(sql);

                // Default values for the result list.
                string[] dummy = new string[8];
                for (int i = 0; i < dummy.Length; i++)
                {
                    dummy[i] = string.Empty;
                }

                // Init the result lits.
                // Filtering items (WHERE clause).
                List<string> filterComparaisons = new List<string>(dummy);
                List<string> filterLogics = new List<string>(dummy);
                List<string> filterFields = new List<string>(dummy);
                List<string> filterComparesTo = new List<string>(dummy);
                List<bool> filterHasValue = new List<bool>(new bool[8].ToList());

                // Sorting items (ORDER BY clause).
                List<string> sortFields = new List<string>(dummy);
                List<bool> sortDirections = new List<bool>(new bool[8].ToList());

                // Chek if SELECT clause exist.
                if (tokens.FindIndex(x => x.TokenID == (int)Tokens.TOKEN_SELECT) != -1)
                {
                    // Check if FROM clause exist.
                    if (tokens.FindIndex(x => x.TokenID == (int)Tokens.TOKEN_FROM) != -1)
                    {
                        // WHERE clause position in tokens list. 
                        int whereIndex = tokens.FindIndex(x => x.TokenID == (int)Tokens.TOKEN_WHERE);
                        // ORDER clause position in tokens list.
                        int orderIndex = tokens.FindIndex(x => x.TokenID == (int)Tokens.TOKEN_ORDER);

                        // Sorting (ORDER clause).
                        if (orderIndex != -1)
                        {
                            // ORDER clause exist.
                            string fieldName = string.Empty;
                            string orderDirection = string.Empty;
                            // Current result index.
                            int index = 0;

                            for (int i = orderIndex + 1; i < tokens.Count; i++)
                            {
                                switch (tokens[i].TokenID)
                                {
                                    case (int)Tokens.TOKEN_ID:
                                        fieldName = tokens[i].Sql;
                                        break;
                                    case (int)Tokens.TOKEN_ASC:
                                    case (int)Tokens.TOKEN_DESC:
                                        orderDirection = tokens[i].Sql.ToUpper();
                                        break;
                                    default:
                                        break;
                                }

                                if (!string.IsNullOrEmpty(fieldName) & !string.IsNullOrEmpty(orderDirection))
                                {
                                    // Add the field and sort direction in the lists.
                                    sortFields[index] = fieldName;
                                    sortDirections[index] = (string.Compare(orderDirection, "ASC") == 0);

                                    fieldName = string.Empty;
                                    orderDirection = string.Empty;

                                    index++;
                                }
                            }
                        }
                        else
                        {
                            orderIndex = tokens.Count;
                        }

                        // Filtering.
                        if (whereIndex != -1)
                        {
                            // Filtering clause exist.

                            // Current result index.
                            int index = 0;

                            // Loop from line after the WHERE clause to line before the ORDER clause (or end of the tokens list).
                            for (int i = whereIndex + 1; i < orderIndex; i++)
                            {
                                // Opening (.
                                if (tokens[i].IsPairMatch && tokens[i].TokenID == 40)
                                {
                                    bool newItem = false;
                                    // Search for the corresponding closing ).
                                    int pairIndex = tokens.FindIndex(x => x.TokenID == 41);

                                    if (pairIndex > i)
                                    {
                                        int j = i + 1;

                                        // List of elements present between the ().
                                        List<string> fields = new List<string>();
                                        List<string> comparaisons = new List<string>();
                                        List<string> values = new List<string>();
                                        List<string> logics = new List<string>();

                                        int k = -1;
                                        newItem = false;

                                        for (; j < pairIndex; j++)
                                        {
                                            switch (tokens[j].TokenID)
                                            {
                                                case (int)Tokens.TOKEN_ID:
                                                    fields.Add(tokens[j].Sql.Trim());
                                                    newItem = false;
                                                    k++;
                                                    break;
                                                case (int)Tokens.TOKEN_IS:
                                                case (int)Tokens.TOKEN_NOT:
                                                case (int)Tokens.TOKEN_NULL:
                                                case (int)Tokens.TOKEN_LIKE:
                                                    if (comparaisons.Count() == k & !newItem)
                                                    {
                                                        comparaisons.Add(tokens[j].Sql.Trim().ToUpper());
                                                        values.Add(string.Empty);
                                                        newItem = true;
                                                    }
                                                    else
                                                    {
                                                        comparaisons[k] = $"{comparaisons[k]} {tokens[j].Sql.Trim().ToUpper()}";
                                                    }
                                                    break;
                                                case (int)Tokens.TOKEN_AND:
                                                case (int)Tokens.TOKEN_OR:
                                                    logics.Add(tokens[j].Sql.Trim().ToUpper());
                                                    break;
                                                case '=':
                                                case '<':
                                                case '>':
                                                    if (comparaisons.Count() == k & !newItem)
                                                    {
                                                        comparaisons.Add(tokens[j].Sql.Trim());
                                                        newItem = true;
                                                    }
                                                    else
                                                    {
                                                        comparaisons[k] = $"{comparaisons[k]}{tokens[j].Sql.Trim()}";
                                                    }
                                                    break;
                                                case (int)Tokens.TOKEN_STRING:
                                                    values.Add(tokens[j].Sql.Trim());
                                                    break;
                                                default:
                                                    break;
                                            }

                                        }

                                        // Add the last logic to complete the list.
                                        logics.Add(string.Empty);

                                        if (fields.Count() == 2)
                                        {
                                            // Detects if the content between the () is of type 'IS NOT NULL' or 'IS NULL' of a text field.
                                            if (string.CompareOrdinal(fields[0], fields[1]) == 0)
                                            {
                                                if (string.CompareOrdinal(comparaisons[0], "IS NOT NULL") == 0)
                                                {
                                                    // 'IS NOT NULL' comparaison.
                                                    if ((string.CompareOrdinal(comparaisons[1], "<>") == 0 & string.CompareOrdinal(values[1], "''") == 0) ||
                                                        (string.IsNullOrEmpty(comparaisons[1]) & string.IsNullOrEmpty(values[1])))
                                                    {
                                                        // Add the item details to the lists.
                                                        filterFields[index] = fields[0];
                                                        filterComparaisons[index] = comparaisons[0];
                                                        filterComparesTo[index] = values[0];
                                                    }
                                                }
                                                else if (string.CompareOrdinal(comparaisons[0], "IS NULL") == 0)
                                                {
                                                    // 'IS NULL' comparaison.
                                                    if ((string.CompareOrdinal(comparaisons[1], "=") == 0 & string.CompareOrdinal(values[1], "''") == 0) ||
                                                        (string.IsNullOrEmpty(comparaisons[1]) & string.IsNullOrEmpty(values[1])))
                                                    {
                                                        // Add the item details to the lists.
                                                        filterFields[index] = fields[0];
                                                        filterComparaisons[index] = comparaisons[0];
                                                        filterComparesTo[index] = values[0];
                                                    }
                                                }
                                                else
                                                {
                                                    // Other comparaison.
                                                    for (int l = 0; l < fields.Count(); l++)
                                                    {
                                                        // Add the item details to the lists.
                                                        filterFields[index] = fields[l];
                                                        filterComparaisons[index] = comparaisons[l];
                                                        filterComparesTo[index] = values[l];
                                                        filterLogics[index] = logics[l];
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Different field type.
                                                for (int l = 0; l < fields.Count(); l++)
                                                {
                                                    // Add the item details to the lists.
                                                    filterFields[index] = fields[l];
                                                    filterComparaisons[index] = comparaisons[l];
                                                    filterComparesTo[index] = values[l];
                                                    filterLogics[index] = logics[l];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Different fields.
                                            for (int l = 0; l < fields.Count(); l++)
                                            {
                                                filterFields[index] = fields[l];
                                                filterComparaisons[index] = comparaisons[l];
                                                filterComparesTo[index] = values[l];
                                                filterLogics[index] = logics[l];
                                            }
                                        }

                                        i = j;
                                    }

                                }
                                else
                                {
                                    // Handling of tokens not contained between ().
                                    switch (tokens[i].TokenID)
                                    {
                                        case (int)Tokens.TOKEN_ID:
                                            filterFields[index] = tokens[i].Sql.Trim();
                                            break;
                                        case (int)Tokens.TOKEN_IS:
                                        case (int)Tokens.TOKEN_NOT:
                                        case (int)Tokens.TOKEN_NULL:
                                        case (int)Tokens.TOKEN_LIKE:
                                            filterComparaisons[index] = $"{filterComparaisons[index]} {tokens[i].Sql.ToUpper()}";
                                            break;
                                        case (int)Tokens.TOKEN_AND:
                                        case (int)Tokens.TOKEN_OR:                                        
                                            filterLogics[index] = tokens[i].Sql.ToUpper();
                                            index++;
                                            break;
                                        case '=':
                                        case '<':
                                        case '>':
                                                filterComparaisons[index] = $"{filterComparaisons[index]}{tokens[i].Sql}";
                                            break;
                                        case (int)Tokens.TOKEN_STRING:
                                        case (int)Tokens.TOKEN_INTEGER:
                                            filterComparesTo[index] = tokens[i].Sql.Trim();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                        }

                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"The SQL code does not contain a FROM clause. The minimum code for a valid SQL query is: SELECT * FROM [Sheet1$]");
                    }
                }
                else
                {
                    Console.WriteLine($"The SQL code does not contain a SELECT clause. The minimum code for a valid SQL query is: SELECT * FROM [Sheet1$]");
                }


                //Console.WriteLine("Filtering");
                //Console.WriteLine(new string('-', 50));
                //for (int i = 0; i < filterComparaisons.Count; i++)
                //{
                //    Console.WriteLine($"{filterFields[i].PadRight(15)} {filterComparaisons[i].PadRight(12)} {filterComparesTo[i].PadRight(15)} {filterLogics[i].PadLeft(5)}");
                //}
                //Console.WriteLine("Sorting");
                //Console.WriteLine(new string('-', 50));
                //for (int i = 0; i < filterComparaisons.Count; i++)
                //{
                //    Console.WriteLine($"{sortFields[i].PadRight(40, ' ')} {sortDirections[i].ToString().PadLeft(9, ' ')}");
                //}
                //Console.WriteLine(new string('=', 50));
                //Console.ReadKey();  

            }
            else
            { 
                foreach (ParseError parseError in parseErrors)
                {
                    Console.WriteLine(parseError.Message);
                }
            }

            return success;
        }

        private bool ParseSQL(string sql)
        {
            ParseOptions parseOptions = new ParseOptions();
            Scanner scanner = new Scanner(parseOptions);

            int state = 0;
            int lastTokenEnd = -1;
            int token;

            List<TokenInfo> tokens = new List<TokenInfo>();

            scanner.SetSource(sql, 0);

            while ((token = scanner.GetNext(ref state, out int start, out int end, out bool isPairMatch, out bool isExecAutoParamHelp)) != (int)Tokens.EOF)
            {
                TokenInfo tokenInfo =
                    new TokenInfo()
                    {
                        Start = start,
                        End = end,
                        IsPairMatch = isPairMatch,
                        IsExecAutoParamHelp = isExecAutoParamHelp,
                        Sql = sql.Substring(start, end - start + 1),
                        TokenText = (Tokens)token,
                        TokenID = token
                    };

                tokens.Add(tokenInfo);

                lastTokenEnd = end;
            }

            TokenInfo item = tokens.Single(x => x.TokenID == (int)Tokens.TOKEN_WHERE);

            if (item != null)
            {
                Console.WriteLine($"Where clause present");
            }

            item = tokens.Single(x => x.TokenID == (int)Tokens.TOKEN_ORDER);

            if (item != null)
            {
                Console.WriteLine($"Order by clause present");
            }

            return true;
        }

        /// <summary>
        /// Parse the SQL code and create the list of tokens.
        /// </summary>
        /// <param name="sql">Code SQL to be parsed.</param>
        /// <returns>LIst of all tokens.</returns>
        private static List<TokenInfo> ParseSql(string sql)
        {
            ParseOptions parseOptions = new ParseOptions();
            Scanner scanner = new Scanner(parseOptions);

            int state = 0;
            int token;

            List<TokenInfo> tokens = new List<TokenInfo>();

            scanner.SetSource(sql, 0);

            while ((token = scanner.GetNext(ref state, out int start, out int end, out bool isPairMatch, out bool isExecAutoParamHelp)) != (int)Tokens.EOF)
            {
                TokenInfo tokenInfo =
                    new TokenInfo()
                    {
                        Start = start,
                        End = end,
                        IsPairMatch = isPairMatch,
                        IsExecAutoParamHelp = isExecAutoParamHelp,
                        Sql = sql.Substring(start, end - start + 1),
                        TokenText = (Tokens)token,
                        TokenID = token
                    };

                tokens.Add(tokenInfo);
            }

            return tokens;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    public class TokenInfo
    {
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsPairMatch { get; set; }

        public bool IsExecAutoParamHelp { get; set; }
        public string Sql { get; set; }
        public Tokens TokenText { get; set; }
        public int TokenID { get; set; }
    }
}
