using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DeletePrefs : Editor
{
    [MenuItem("Custom/Delete All Prefs")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All PlayerPrefs Deleted.");
    }
}
