//code by Egor Grishechko
//https://egorikas.com/max-and-min-heap-implementation-with-csharp/
//then modified by me, Picchi Yoan to support my data type
using System.Collections;
using UnityEngine;

public class MaxHeap
{
	private readonly DataPoint[] _elements;
	private int _size;


	public MaxHeap(int size)
	{
		_elements = new DataPoint[size];
	}

	private int GetLeftChildIndex(int elementIndex) => 2 * elementIndex + 1;
	private int GetRightChildIndex(int elementIndex) => 2 * elementIndex + 2;
	private int GetParentIndex(int elementIndex) => (elementIndex - 1) / 2;

	private bool HasLeftChild(int elementIndex) => GetLeftChildIndex(elementIndex) < _size;
	private bool HasRightChild(int elementIndex) => GetRightChildIndex(elementIndex) < _size;
	private bool IsRoot(int elementIndex) => elementIndex == 0;

	private DataPoint GetLeftChild(int elementIndex) => _elements[GetLeftChildIndex(elementIndex)];
	private DataPoint GetRightChild(int elementIndex) => _elements[GetRightChildIndex(elementIndex)];
	private DataPoint GetParent(int elementIndex) => _elements[GetParentIndex(elementIndex)];

	private void Swap(int firstIndex, int secondIndex)
	{
		var temp = _elements[firstIndex];
		_elements[firstIndex] = _elements[secondIndex];
		_elements[secondIndex] = temp;
	}

	public bool IsEmpty()
	{
		return _size == 0;
	}

	public DataPoint Peek()
	{
		//if (_size == 0)
			//throw new IndexOutOfRangeException();

		return _elements[0];
	}

	public DataPoint Pop()
	{
		//if (_size == 0)
			//throw new IndexOutOfRangeException();

		var result = _elements[0];
		_elements[0] = _elements[_size - 1];
		_size--;

		ReCalculateDown();

		return result;
	}

	public void Add(DataPoint element)
	{
		//if (_size == _elements.Length)
			//throw new IndexOutOfRangeException();

		_elements[_size] = element;
		_size++;

		ReCalculateUp();
	}
	
	
	public DataPoint[] GetArray(){
		return _elements;
	}
	
	
	
	

	private void ReCalculateDown()
	{
		int index = 0;
		while (HasLeftChild(index))
		{
			var biggerIndex = GetLeftChildIndex(index);
			if (HasRightChild(index) && GetRightChild(index).SensorPower > GetLeftChild(index).SensorPower)
			{
				biggerIndex = GetRightChildIndex(index);
			}

			if (_elements[biggerIndex].SensorPower < _elements[index].SensorPower)
			{
				break;
			}

			Swap(biggerIndex, index);
			index = biggerIndex;
		}
	}

	private void ReCalculateUp()
	{
		var index = _size - 1;
		while (!IsRoot(index) && _elements[index].SensorPower > GetParent(index).SensorPower)
		{
			var parentIndex = GetParentIndex(index);
			Swap(parentIndex, index);
			index = parentIndex;
		}
	}
}