namespace Lutra.Utility;

/// <summary>
/// Implements various stable sorting algorithms as alternatives to Array.Sort, which is not stable.
/// It is important to retain a stable sort order on internal lists, 
/// in order to avoid visual or logical inconsistencies from frame to frame.
/// </summary>
public static class Sorting
{
    /// <summary>
    /// Sorts an array using an insertion sort.
    /// This sorting algorithm is stable, but is not performant for larger arrays.
    /// </summary>
    public static void InsertionSort<T>(T[] array, int startIndex, int endIndex, Comparison<T> comparison)
    {
        T temp;
        int i, j;
        for (i = startIndex + 1; i < endIndex; i++)
        {
            temp = array[i];
            j = i - 1;
            while (j >= startIndex && comparison(array[j], temp) > 0)
            {
                array[j + 1] = array[j];
                j -= 1;
            }
            array[j + 1] = temp;
        }
    }

    /// <summary>
    /// Sorts an array using a bottom-up merge sort.
    /// This sorting algorithm is stable.
    /// It uses a preallocated working array of the same size as the input.
    /// </summary>
    public static void MergeSort<T>(T[] array, T[] work, int count, Comparison<T> comparison)
    {
        MergeSort(array, work, count, 1, comparison);
    }

    /// <summary>
    /// Sorts an array using a simple hybrid insertion and bottom-up merge sort.
    /// This sorting algorithm is stable.
    /// It uses a preallocated working array of the same size as the input.
    /// </summary>
    public static void StableSort<T>(T[] array, T[] work, int count, Comparison<T> comparison)
    {
        // First, insertion sort runs of 16
        int start = 0;
        while (start < count)
        {
            int end = Math.Min(start + 16, count);
            InsertionSort(array, start, end, comparison);
            start = end;
        }

        // Then, merge sort starting at width 16
        if (count > 16)
        {
            MergeSort(array, work, count, 16, comparison);
        }
    }

    private static void MergeSort<T>(T[] array, T[] work, int count, int startingWidth, Comparison<T> comparison)
    {
        bool inWorkArray = false;
        T[] input, output;
        for (int width = startingWidth; width < count; width = 2 * width)
        {
            if (inWorkArray)
            {
                input = work;
                output = array;
            }
            else
            {
                input = array;
                output = work;
            }

            for (int i = 0; i < count; i += 2 * width)
            {
                int right = Math.Min(i + width, count);
                int end = Math.Min(i + 2 * width, count);
                Merge(input, i, right, end, output, comparison);
            }

            inWorkArray = !inWorkArray;
        }

        if (inWorkArray)
        {
            Array.Copy(work, 0, array, 0, count);
        }
    }

    private static void Merge<T>(T[] input, int left, int right, int end, T[] output, Comparison<T> comparison)
    {
        int i = left, j = right;
        for (int k = left; k < end; k++)
        {
            if (i < right && (j >= end || comparison(input[i], input[j]) <= 0))
            {
                output[k] = input[i];
                i += 1;
            }
            else
            {
                output[k] = input[j];
                j += 1;
            }
        }
    }
}
