using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyLogic;

public class FuzzyLogicTest : MonoBehaviour
{
    public bool test;
    [Range(0, 10)] public int voiceRate = 5;
    [Range(0, 10)] public int instrumentalRate = 5;

    private FuzzySystem songRatingFuzzySystem;

    private void Start()
    {
        songRatingFuzzySystem = new FuzzySystem("SongRating");

        /* BEGINNING INDEPENDIENT VARIABLES #################################################################################### */

        // Voice Rate
        FuzzyVariable voiceRateVariable = new FuzzyVariable("VoiceRate");
        voiceRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Bad", one: 0, zero:4));
        voiceRateVariable.AddFuzzySet(new TriangularFuzzySet("Medium", min_zero: 3, one: 5, max_zero: 7));
        voiceRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Good", zero: 6, one: 10));

        songRatingFuzzySystem.AddIndependientSet(voiceRateVariable); // Add Independient variable to System

        // Instrumental Rate
        FuzzyVariable instrumentalRateVariable = new FuzzyVariable("InstrumentalRate");
        instrumentalRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Bad", one: 0, zero: 4));
        instrumentalRateVariable.AddFuzzySet(new TriangularFuzzySet("Medium", min_zero: 3, one: 5, max_zero: 7));
        instrumentalRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Good", zero: 6, one: 10));

        songRatingFuzzySystem.AddIndependientSet(instrumentalRateVariable); // Add Independient variable to System

        /* END INDEPENDIENT VARIABLES */


        /* BEGINNING DEPENDIENT VARIABLES ######################################################################################## */

        // SongRate
        // Instrumental Rate
        FuzzyVariable songRateVariable = new FuzzyVariable("SongRate");
        songRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Bad", one: 0, zero: 4));
        songRateVariable.AddFuzzySet(new TriangularFuzzySet("Medium", min_zero: 3, one: 5, max_zero: 7));
        songRateVariable.AddFuzzySet(new DiagonalLineFuzzySet("Good", zero: 6, one: 10));

        songRatingFuzzySystem.AddDependientSet(songRateVariable); // Add Dependient variable to System

        /* END DEPENDIENT VARIABLES */

        
        /* BEGINNING RULES ######################################################################################################## */
        Dictionary<string, string> antecedents;
        KeyValuePair<string, string> consequent;

        // Rule 1
        antecedents = new Dictionary<string, string>();
        antecedents.Add("VoiceRate", "Good");
        antecedents.Add("InstrumentalRate", "Good");
        consequent = new KeyValuePair<string, string>("SongRate", "Good");
        songRatingFuzzySystem.AddRule(new Rule(antecedents, consequent));

        // Rule 2
        antecedents = new Dictionary<string, string>();
        antecedents.Add("VoiceRate", "Bad");
        consequent = new KeyValuePair<string, string>("SongRate", "Bad");
        songRatingFuzzySystem.AddRule(new Rule(antecedents, consequent));

        // Rule 3
        antecedents = new Dictionary<string, string>();
        antecedents.Add("InstrumentalRate", "Bad");
        consequent = new KeyValuePair<string, string>("SongRate", "Bad");
        songRatingFuzzySystem.AddRule(new Rule(antecedents, consequent));

        // Rule 4
        antecedents = new Dictionary<string, string>();
        antecedents.Add("VoiceRate", "Medium");
        consequent = new KeyValuePair<string, string>("SongRate", "Medium");
        songRatingFuzzySystem.AddRule(new Rule(antecedents, consequent));

        // Rule 4
        antecedents = new Dictionary<string, string>();
        antecedents.Add("InstrumentalRate", "Medium");
        consequent = new KeyValuePair<string, string>("SongRate", "Medium");
        songRatingFuzzySystem.AddRule(new Rule(antecedents, consequent));

        /* END RULES */
    }

    private void Update()
    {
        if(test)
        {
            Dictionary<string, float> inputs = new Dictionary<string, float>();
            inputs.Add("VoiceRate", voiceRate);
            inputs.Add("InstrumentalRate", instrumentalRate);

            Dictionary<string, float> outputs;

            /* Simpliest way to obtain outputs */
            // outputs = songRatingFuzzySystem.FuzzyInferenceSystem(inputs);

            /* Step by step to obtain outputs */
            Dictionary<string, Dictionary<string, float>> fuzzyInputs = songRatingFuzzySystem.Fuzzify(inputs);
            Dictionary<string, List<KeyValuePair<string, float>>> fuzzyOutputs = songRatingFuzzySystem.FuzzyOperator(fuzzyInputs);
            outputs = songRatingFuzzySystem.Inference(fuzzyOutputs);

            Debug.Log("Song Rate: " + outputs["SongRate"]);
        }
    }
}
