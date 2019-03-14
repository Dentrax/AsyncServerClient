#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;

namespace ServerConsole
{
    public class Functions
    {
        public static bool ToBoolean(string strValue)
        {
            try
            {
                switch (strValue.ToLower())
                {
                    case "true":
                        return true;
                    case "t":
                        return true;
                    case "1":
                        return true;
                    case "0":
                        return false;
                    case "false":
                        return false;
                    case "f":
                        return false;
                    default:
                        return false;
                        //throw new InvalidCastException("You can't cast a weird value to a bool!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
