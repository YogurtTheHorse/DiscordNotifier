namespace YogurtTheCommunity.Utils;

public static class TextUtils
{
    private const string BoolsTrue = "ty+1д";
    private const string BoolsFalse = "fn-0н";
    
    public static string Escape(this string s)
    {
        return s.Replace("\"", "\\\"");
    }

    public static bool? AsBool(this string s)
    {
        if (s.Length == 0) return null;

        var c = s.ToLowerInvariant()[0];
        
        if (BoolsTrue.Contains(c)) return true;
        if (BoolsFalse.Contains(c)) return false;

        return null;
    }
}