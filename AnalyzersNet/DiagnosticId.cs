using Qowaiv.Customization;

namespace AnalyzersNet;

/// <summary>Represents the Id of the <see cref="DiagnosticDescriptor"/>.</summary>
[Id<Behavior, string>]
public readonly partial struct DiagnosticId
{
    /// <summary>The (optional) prefix.</summary>
    public string? Prefix => m_Value is null ? null : Behavior.Split(m_Value).Text;

    /// <summary>The (optional) numeric value.</summary>
    public int? Numeric => m_Value is null ? null : Behavior.Split(m_Value).Numeric;

    private sealed class Behavior : StringIdBehavior
    {
        /// <inheritdoc />
        [DoesNotReturn]
        public override string NextId() => throw new NotSupportedException();

        /// <inheritdoc />
        [Pure]
        public override int Compare(string? x, string? y)
        {
            var l = Split(x);
            var r = Split(y);

            var compare = l.Text.CompareTo(r.Text);
            return compare is 0 && l.Numeric is { } ln && r.Numeric is { } rn
                ? ln.CompareTo(rn)
                : compare;
        }

        [Pure]
        public static (string Text, int? Numeric) Split(string? value)
        {
            if (value is not { Length: > 0 }) return (string.Empty, 0);

            var i = value.Length - 1;
            while (i > 0 && char.IsAsciiDigit(value[i])) i--;

            return (value[..(i + 1)], int.TryParse(value[(i + 1)..], out var n) ? n : null);
        }
    }
}
