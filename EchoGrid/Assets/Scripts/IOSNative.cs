using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public static class IOSNative
{  
    [DllImport("__Internal")]
    private static extern void _ex_ShowThree(string title, string message, string yes, string no, string na);

    [DllImport("__Internal")]
    private static extern void _ex_ShowTwo(string title, string message, string yes, string no);

    [DllImport("__Internal")]
    private static extern void _ex_ShowOne(string title, string message, string ok);

    [DllImport("__Internal")]
    private static extern void _ex_ShowTwoG(string title, string message, string yes, string no);

    [DllImport("__Internal")]
    private static extern void _ex_ShowOneG(string title, string message, string ok);


    public static void ShowThree(string title, string message, string yes, string no, string na)
    {
        _ex_ShowThree(title, message, yes, no, na);
    }

    public static void ShowTwo(string title, string message, string yes, string no)
    {
        _ex_ShowTwo(title, message, yes, no);
    }

    public static void ShowOne(string title, string message, string ok)
    {
        _ex_ShowOne(title, message, ok);
    }

    public static void ShowTwoG(string title, string message, string yes, string no)
    {
        _ex_ShowTwoG(title, message, yes, no);
    }

    public static void ShowOneG(string title, string message, string ok)
    {
        _ex_ShowOneG(title, message, ok);
    }
}
