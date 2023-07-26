using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * 우선순위 큐를 사용하는 이유 AStar의 오픈리스트에 들어간 노드에서 코스트가 가장 적은 노드를 찾아야하기 때문
 * min heap으로 구현한 우선순위 큐를 사용한다. 배열이나 링크드 리스트의 경우 정렬을 할때 매우 큰 비용이 들어갈 수 있다.
 * 리스트의 경우 가장 앞부분에 코스트가 적은 노드를 넣을 경우 뒤에 있는 요소들을 모두 한칸 씩 뒤로 밀어주어야 해서
 * 효율이 매우 안좋아진다. 링크드 리스트의 경우 삽입을 할때 최악의 경우 모든 데이터를 순회할 수 있으므로 효율이 좋지않다.
 * 우선순위 큐의 경우 트리의 구조로 이루어져 있기 때문에 insert 할때 깊이가 깊어질때마다 연산횟수가 1회 늘어나므로 매우 효율적이다.
 * 힙 트리의 구조로 구현한다. 자식 노드의 위치는 항상 확정적이기 때문에 배열로 구현이 가능하다. index 접근 가능
*/
public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> _heap = new List<T>();
    public int Count { get => _heap.Count; }

    public void Clear()
    {
        _heap.Clear();
    }

    public void Push(T t)
    {
        _heap.Add(t);

        int nowIndex = _heap.Count - 1;

        // 가장 끝 노드에 원소 삽입 후 순차적으로 자신의 위치를 찾아간다.
        while (nowIndex > 0)
        {
            int parentIndex = (nowIndex - 1) / 2;

            if (_heap[nowIndex].CompareTo(_heap[parentIndex]) < 0) // 특정 조건에 만족하는 경우 자신의 위치가 된다.
                break;

            T temp = _heap[nowIndex];
            _heap[nowIndex] = _heap[parentIndex];
            _heap[parentIndex] = temp;

            nowIndex = parentIndex;
        }
    }
    public T Pop()
    {
        // 항상 루트 노드는 특정 조건에 의해 정렬된 값이므로 루트 노드를 반환 해준다.
        T ret = _heap[0];

        int lastIndex = _heap.Count - 1;
        _heap[0] = _heap[lastIndex];
        _heap.RemoveAt(lastIndex--);

        int nowIndex = 0;

        while (true)
        {
            int leftIndex  = nowIndex * 2 + 1;
            int rightIndex = nowIndex * 2 + 2;

            int nextIndex = nowIndex;

            if (leftIndex <= lastIndex && _heap[nextIndex].CompareTo(_heap[leftIndex]) < 0)
                nextIndex = leftIndex;
            if (rightIndex <= lastIndex && _heap[nextIndex].CompareTo(_heap[rightIndex]) < 0)
                nextIndex = rightIndex;

            if (nowIndex == nextIndex)
                break;

            T temp = _heap[nowIndex];
            _heap[nowIndex]  = _heap[nextIndex];
            _heap[nextIndex] = temp;

            nowIndex = nextIndex;
        }

        return ret;
    }

    public T this[int index]
    {
        get => _heap[index];
    }
}
