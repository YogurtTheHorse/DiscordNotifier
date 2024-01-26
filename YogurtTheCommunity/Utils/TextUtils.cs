namespace YogurtTheCommunity.Utils;

public static class TextUtils
{
    public static string Escape(this string s)
    {
        return s.Replace("\"", "\\\"");
    }
}