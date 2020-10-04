using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ProceduralGen
{
    [Serializable]
    public struct WeightedProbabilityString
    {
        public float weight;
        public string name;
    }
    [CreateAssetMenu(fileName = "NameGenerator", menuName = "MapGeneration/NameGenerator", order = 10)]
    public class NameGen : ScriptableObject
    {
        public string[] FirstNames;
        public string[] LastNames;
        public WeightedProbabilityString[] Titles;

        public string GenerateName()
        {
            var randomGen = new System.Random();
            var title = SelectRandomlyBasedOnWeights(Titles, randomGen);
            if (title.Length > 0)
            {
                title = title + ' ';
            }
            return $"{title}{SelectRandomFromList(FirstNames, randomGen)} {SelectRandomFromList(LastNames, randomGen)}";
        }

        public static string SelectRandomFromList(string[] choices, System.Random randomGen)
        {
            var nextIndex = randomGen.Next(0, choices.Length);
            return choices[nextIndex];
        }

        public static string SelectRandomlyBasedOnWeights(WeightedProbabilityString[] choices, System.Random randomGen)
        {
            var weightsTotal = choices.Sum(x => x.weight);
            var randomValScaled = randomGen.NextDouble() * weightsTotal;
            var currentChoicePoint = 0f;
            var currentChoiceIndex = -1;
            do
            {
                currentChoiceIndex++;
                if (currentChoiceIndex >= choices.Length)
                {
                    throw new Exception("random algo failed");
                }
                currentChoicePoint += choices[currentChoiceIndex].weight;
            } while (currentChoicePoint < randomValScaled);
            return choices[currentChoiceIndex].name;
        }
    }
}
