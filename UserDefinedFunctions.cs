using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static bool RxMatch( string pattern, string source, int match_case = 0 )
    {
        var regex = GetRegex( pattern, match_case );
        return regex.IsMatch( source );
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static string RxReplace( string pattern, string source, string replacement, int match_case = 0 )
    {
        var regex = GetRegex( pattern, match_case );
        return regex.Replace( source, replacement );
    }

    private static Regex GetRegex( string pattern, int match_case )
    {
        var search_key = pattern + "~" + match_case;
        lock ( _regexes_lock )
        {
            if ( _regexes.ContainsKey( search_key ) )
            {
                return _regexes[ search_key ];
            }
            
            var regex = CreateRegex( pattern, match_case );
            _regexes[ search_key ] = regex;
            return regex;
        }
    }

    private static Regex CreateRegex( string pattern, int match_case )
    {
        var options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        if ( match_case == 1 )
        {
            options |= RegexOptions.IgnoreCase;
        }

        var regex = new Regex( pattern, options );
        return regex;
    }

    private static readonly Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>(); 
    private static readonly object _regexes_lock = new object();
}
