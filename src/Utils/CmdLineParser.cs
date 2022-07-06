using System;
using System.Text.RegularExpressions;

namespace MinecraftWebExporter.Utils;

/// <summary>
/// The cli parser can read the command line string array and parse them into parameter with values.
/// A parameter is defined by a parameter name and optional values. Tha name has to match a certain pattern that can
/// be defined by a regular expression.
/// If an argument matches the parameter name expression it is considered as parameter. All following arguments are
/// read as values for this parameter as long as they do not match the parameter name expression. Otherwise the next
/// parameter starts.
/// The parser starts in an empty state. You have to call <see cref="Next"/> before you can read the first parameter
/// with its values. <see cref="Next"/> returns <c>true</c> if there is a new parameter to parse.
/// </summary>
/// <example>
/// <code>
/// var arguments = new string[] { "-param1", "value1", "value2", "-param2", "-param3", "1", "2.0", "-3" };
/// var parser = new CmdLineParser(arguments);
/// while (parser.Next())
/// {
///     Console.WriteLine($"Parameter name: {parser.ParameterName} - Values: {parser.ValueCount}");
///     for (var i=0; i &lt; parser.ValueCount; i++)
///     {
///         Console.WriteLine($" - Value[{i}]: {parser.GetValue(i)}");
///     }
/// }
/// </code>
/// Output:
/// <code>
/// Parameter name: -param1 - Values: 2
///  - Value[0]: value1
///  - Value[1]: value2
/// Parameter name: -param2 - Values: 0
/// Parameter name: -param3 - Values: 3
///  - Value[0]: 1
///  - Value[1]: 2.0
///  - Value[2]: -3
/// </code>
/// </example>
public struct CmdLineParser
{
    private readonly string[] m_Arguments;
    private readonly Regex m_ParameterNameRegex;

    /// <summary>
    /// The default regex accept any argument name that starts with a dash and a letter combination.
    /// Underscores and dashes are allowed.
    /// The argument name must start with a letter, so negative numbers in the value list are not mistaken as arguments.
    /// </summary>
    public static readonly Regex DefaultArgumentRegex = new(@"^\-[a-zA-Z\-][a-zA-Z0-9_\-]*$");

    /// <summary>
    /// Creates a command line parser for the given <paramref name="arguments"/>.
    /// An argument is defined by <see cref="DefaultArgumentRegex"/>.
    /// </summary>
    /// <param name="arguments">The argument array. You can use <c>Environment.GetCommandLineArgs()</c> for example.</param>
    public CmdLineParser(string[] arguments) : this(arguments, DefaultArgumentRegex)
    {
    }

    /// <summary>
    /// Creates a command line parser for the given <see cref="arguments"/>.
    /// An argument is defined by the custom <paramref name="regex"/>.
    /// </summary>
    /// <param name="arguments">The argument array. You can use <c>Environment.GetCommandLineArgs()</c> for example.</param>
    /// <param name="regex">The regex that is used to define an argument name.</param>
    public CmdLineParser(string[] arguments, Regex regex)
    {
        m_Arguments = arguments;
        m_ParameterNameRegex = regex;

        ParameterIndex = -1;
        ValueCount = 0;
    }
    
    /// <summary>
    /// Gets the index of the current parameter in the arguments array
    /// </summary>
    public int ParameterIndex { get; private set; }
    
    /// <summary>
    /// Gets the name of the current parameter
    /// </summary>
    public string ParameterName => m_Arguments[ParameterIndex];
    
    /// <summary>
    /// Reads the next parameter.
    /// Returns false if there is no parameter left.
    /// </summary>
    /// <returns></returns>
    public bool Next()
    {
        var start = ParameterIndex + ValueCount + 1;
        if (start >= m_Arguments.Length) return false;

        // Check if the first argument is a valid parameter name
        var value = m_Arguments[start];
        if (!m_ParameterNameRegex.IsMatch(value))
        {
            throw new ArgumentException($"The argument '{value}' doesn'n match the regular expresion.", nameof(m_Arguments));
        }
        
        var count = 0;
        while (start + count + 1 < m_Arguments.Length)
        {
            value = m_Arguments[start + count + 1];
            if (m_ParameterNameRegex.IsMatch(value))
            {
                break;
            }

            count++;
        }

        ParameterIndex = start;
        ValueCount = count;
        return true;
    }
    
    /// <summary>
    /// Returns the value for the current parameter
    /// </summary>
    /// <param name="index">The zero-based between 0 and <see cref="ValueCount"/></param>
    /// <returns>The text value</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public string GetValue(int index)
    {
        if (index < 0 || index >= ValueCount) throw new IndexOutOfRangeException();
        return m_Arguments[ParameterIndex + index + 1];
    }

    /// <summary>
    /// Gets the number of values for the current parameter
    /// </summary>
    public int ValueCount { get; private set; }

    /// <summary>
    /// Returns an span with all values for the current parameter
    /// </summary>
    public ReadOnlySpan<string> Values =>
        new(m_Arguments, ParameterIndex + 1, ValueCount);
}