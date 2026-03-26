
using System.Collections.Generic;
using System.Linq;

public class NumberPool
{
    private Queue<int> _free;
    private int _max;

    public NumberPool(List<int> usedNumbers)
    {
        usedNumbers.Sort();
        _max = usedNumbers.LastOrDefault();

        _free = new Queue<int>();

        // добавляем только пропуски
        int index = 0;
        for (int i = 1; i < _max; i++)
        {
            if (index < usedNumbers.Count && usedNumbers[index] == i)
                index++;
            else
                _free.Enqueue(i);
        }
    }

    public int GetNext()
    {
        if (_free.Count > 0)
            return _free.Dequeue();

        _max++;
        return _max;
    }
}