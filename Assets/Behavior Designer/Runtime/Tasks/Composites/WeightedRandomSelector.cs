using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Similar to the random selector, but the order in which child tasks are executed is determined by their assigned probabilities (weights). " +
                     "A higher weight means a child is more likely to be attempted first. If a child fails, the next child is chosen randomly from the remaining ones, " +
                     "respecting their relative weights. Returns success as soon as one child succeeds, or failure if all children fail.")]
    [TaskIcon("{SkinColor}WeightedRandomSelectorIcon.png")]
    public class WeightedRandomSelector : Composite
    {
        [Tooltip("The probability weights for each child task. Must have the same length as the number of children. " +
                 "Weights are relative, they do not need to sum to 1. Negative values are treated as 0.")]
        public float[] probabilities = new float[0];

        [Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        // A list of indexes of every child task. Used to rebuild execution order.
        private List<int> childIndexList = new List<int>();
        // The random child index execution order generated based on weights.
        private Stack<int> childrenExecutionOrder = new Stack<int>();
        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;
        // Internal copy of probabilities to work with, aligned to child count.
        private List<float> internalProbabilities = new List<float>();

        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed)
            {
                Random.InitState(seed);
            }

            // Populate child index list.
            childIndexList.Clear();
            for (int i = 0; i < children.Count; ++i)
            {
                childIndexList.Add(i);
            }

            // Prepare probability list, adjusting for mismatched lengths.
            internalProbabilities.Clear();
            if (probabilities == null || probabilities.Length == 0)
            {
                // Default to equal weight if no probabilities are given.
                for (int i = 0; i < children.Count; i++)
                {
                    internalProbabilities.Add(1f);
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (i < probabilities.Length)
                    {
                        // Clamp negative values to 0.
                        internalProbabilities.Add(Mathf.Max(0f, probabilities[i]));
                    }
                    else
                    {
                        // If the provided array is too short, fill the rest with 1.
                        internalProbabilities.Add(1f);
                    }
                }
            }
        }

        public override void OnStart()
        {
            // Generate the weighted random execution order.
            GenerateWeightedOrder();
        }

        public override int CurrentChildIndex()
        {
            // Peek will return the index at the top of the stack.
            return childrenExecutionOrder.Peek();
        }

        public override bool CanExecute()
        {
            // Continue execution if no task has returned success and indexes still exist on the stack.
            return childrenExecutionOrder.Count > 0 && executionStatus != TaskStatus.Success;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            // Pop the top index from the stack and record the execution status.
            if (childrenExecutionOrder.Count > 0)
            {
                childrenExecutionOrder.Pop();
            }
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Start from the beginning on an abort.
            childrenExecutionOrder.Clear();
            executionStatus = TaskStatus.Inactive;
            GenerateWeightedOrder();
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
            childrenExecutionOrder.Clear();
        }

        public override void OnReset()
        {
            // Reset the public properties back to their original values.
            seed = 0;
            useSeed = false;
            probabilities = new float[0];
        }

        /// <summary>
        /// Generates the order in which child tasks will be executed using weighted random sampling without replacement.
        /// Higher weights increase the chance of a child being placed early in the order.
        /// </summary>
        private void GenerateWeightedOrder()
        {
            childrenExecutionOrder.Clear();

            // Work with a copy of the indices and their corresponding weights.
            List<int> availableIndices = new List<int>(childIndexList);
            List<float> availableWeights = new List<float>(internalProbabilities);

            while (availableIndices.Count > 0)
            {
                // Calculate the sum of weights for the remaining children.
                float totalWeight = 0f;
                for (int i = 0; i < availableWeights.Count; i++)
                {
                    totalWeight += availableWeights[i];
                }

                int chosenIndex;
                if (totalWeight <= 0f)
                {
                    // If all remaining weights are zero, fall back to uniform random selection.
                    int randomIndex = Random.Range(0, availableIndices.Count);
                    chosenIndex = randomIndex;
                }
                else
                {
                    // Weighted random selection.
                    float randomValue = Random.Range(0f, totalWeight);
                    float cumulative = 0f;
                    int selected = 0;
                    for (int i = 0; i < availableWeights.Count; i++)
                    {
                        cumulative += availableWeights[i];
                        if (randomValue <= cumulative)
                        {
                            selected = i;
                            break;
                        }
                    }
                    chosenIndex = selected;
                }

                // Push the actual child index onto the execution stack and remove it from the available lists.
                childrenExecutionOrder.Push(availableIndices[chosenIndex]);
                availableIndices.RemoveAt(chosenIndex);
                availableWeights.RemoveAt(chosenIndex);
            }
        }
    }
}