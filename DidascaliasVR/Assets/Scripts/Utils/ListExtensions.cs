using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static Random _random = new Random();

    /// <summary>
    /// Mezcla los elementos de la lista de forma aleatoria (In-place).
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            // Intercambiar elementos
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Devuelve un elemento aleatorio de la lista.
    /// </summary>
    public static T Next<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.InvalidOperationException("La lista está vacía o es nula.");
        }

        return list[_random.Next(list.Count)];
    }
}
