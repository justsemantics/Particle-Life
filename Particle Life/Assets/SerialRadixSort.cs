using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialRadixSort
{
    public uint[] Sort(uint[] unsortedData)
    {  
        //uint[] sortedData = new uint[unsortedData.Length];

        //counting sort each byte starting from least significant
        for(int i = 0; i < 4; i++)
        {
            unsortedData = CountingSortByByte(unsortedData, i);
        }

        return unsortedData;
    }

    public byte[] CountingSort(byte[] unsortedData)
    {
        byte[] sortedData = new byte[unsortedData.Length];

        uint[] memory = new uint[256];

        //count number of entries with each potential value
        for (int i = 0; i < unsortedData.Length; i++)
        {
            memory[unsortedData[i]]++;
        }

        //update memory to include number of previous elements as well
        for (int i = 1; i < memory.Length; i++)
        {
            memory[i] = memory[i] + memory[i - 1];
        }

        //iterate through data once again and insert at indicated index
        for(int i = 0; i < unsortedData.Length; i++)
        {
            sortedData[memory[unsortedData[i]]] = unsortedData[i];
            memory[unsortedData[i]] -= 1;
        }

        return sortedData;
    }

    public int getByteValue(uint value, int relevantByte)
    {
        byte byteValue = (byte)(value >> (8 * relevantByte));
        int intValue =  (int)byteValue;
        return intValue;
    }

    public uint[] CountingSortByByte(uint[] unsortedData, int relevantByte)
    {
        uint[] sortedData = new uint[unsortedData.Length];

        uint[] memory = new uint[256];

        //count number of entries with each potential value
        for (int i = 0; i < unsortedData.Length; i++)
        {
            int memoryPosition = getByteValue(unsortedData[i], relevantByte);
            memory[memoryPosition]++;
        }

        //update memory to include number of following elements as well
        for (int i = memory.Length - 2; i >= 0; i--)
        {
            memory[i] = memory[i] + memory[i + 1];
        }

        //iterate through data once again and insert at indicated index
        for (int i = 0; i < unsortedData.Length; i++)
        {
            int memoryPosition = getByteValue(unsortedData[i], relevantByte);
            sortedData[sortedData.Length - (memory[memoryPosition])] = unsortedData[i];
            memory[memoryPosition] -= 1;
        }

        return sortedData;
    }

    public Agent[] Sort(Agent[] agents)
    {
        for(int i = 0; i < 4; i++)
        {
            agents = CountingSortByByte(agents, i);
        }

        return agents;
    }

    public Agent[] CountingSortByByte(Agent[] agents, int relevantByte)
    {
        Agent[] sortedAgents = new Agent[agents.Length];

        uint[] memory = new uint[256];

        //count number of entries with each potential value
        for (int i = 0; i < agents.Length; i++)
        {
            int memoryPosition = getByteValue(agents[i].mortonCode, relevantByte);
            memory[memoryPosition]++;
        }

        //update memory to include number of following elements as well
        for (int i = memory.Length - 2; i >= 0; i--)
        {
            memory[i] = memory[i] + memory[i + 1];
        }

        //iterate through data once again and insert at indicated index
        for (int i = 0; i < agents.Length; i++)
        {
            int memoryPosition = getByteValue(agents[i].mortonCode, relevantByte);
            sortedAgents[sortedAgents.Length - (memory[memoryPosition])] = agents[i];
            memory[memoryPosition] -= 1;
        }

        return sortedAgents;
    }

    public void TestCountingSort(int numElements)
    {
        byte[] unsortedData = new byte[numElements];
        System.Random random = new System.Random();
        random.NextBytes(unsortedData);

        byte[] sortedData = CountingSort(unsortedData);

        for(int i = 0; i < numElements; i++)
        {
            Debug.Log(string.Format("Element {0}: {1} | {2}",
                i, unsortedData[i], sortedData[i]));
        }
    }

    public void TestRadixSort(int numElements)
    {
        uint[] unsortedData = new uint[numElements];
        System.Random random = new System.Random();
        for (int i = 0; i < numElements; i++)
        {
            unsortedData[i] = (uint)random.Next() + int.MaxValue;
        }

        uint[] sortedData = Sort(unsortedData);

        for (int i = 0; i < numElements; i++)
        {
            Debug.Log(string.Format("Element {0}: {1} | {2}",
                i, unsortedData[i], sortedData[i]));
        }
    }
}

