using System;
using System.Text;
using UnityEngine;

public class ArrayUtils
{
    public static String ToString(int[] array)
    {
        StringBuilder result = new StringBuilder("[");

        for (int i = 0; i < array.Length; i++)
        {
            result.Append($"{array[i]}");
            if (i < array.Length - 1)
            {
                result.Append(",");
            }
        }

        return result.Append("]").ToString();
    }
}
