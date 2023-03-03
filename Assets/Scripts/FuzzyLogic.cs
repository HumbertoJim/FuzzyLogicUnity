using System.Collections;
using System.Collections.Generic;
using System;

namespace FuzzyLogic
{
    public class FuzzySystem
    {
        string name;
        Dictionary<string, FuzzyVariable> independientSets;
        Dictionary<string, FuzzyVariable> dependientSets;
        Dictionary<string, List<Rule>> rules;

        public FuzzySystem(string name)
        {
            this.name = name;
            independientSets = new Dictionary<string, FuzzyVariable>();
            dependientSets = new Dictionary<string, FuzzyVariable>();
            rules = new Dictionary<string, List<Rule>>();
        }

        public void AddIndependientSet(FuzzyVariable set)
        {
            if (independientSets.ContainsKey(set.Name()))
            {
                throw new ArgumentException("Independient Fuzzy Set '" + set.Name() + "' already added");
            }
            independientSets.Add(set.Name(), set);
        }

        public void AddDependientSet(FuzzyVariable set)
        {
            if (dependientSets.ContainsKey(set.Name()))
            {
                throw new ArgumentException("Dependient Fuzzy Set '" + set.Name() + "' already added");
            }
            dependientSets.Add(set.Name(), set);
            rules.Add(set.Name(), new List<Rule>());
        }

        public void AddRule(Rule rule)
        {
            if (dependientSets.ContainsKey(rule.Consequent().Key))
            {
                if (!dependientSets[rule.Consequent().Key].FuzzySetExists(rule.Consequent().Value))
                {
                    throw new ArgumentException("Dependient Fuzzy Set '" + rule.Consequent().Key + "' does not contain a fuzzySet with name'" + rule.Consequent().Value + "'");
                }
            }
            else
            {
                throw new ArgumentException("Fuzzy System '" + name + "' does not contain a Dependient FuzzyVariable with name '" + rule.Consequent().Key + "'");
            }
            foreach (KeyValuePair<string, string> condition in rule.Antecedents())
            {
                if (independientSets.ContainsKey(condition.Key))
                {
                    if (!independientSets[condition.Key].FuzzySetExists(condition.Value))
                    {
                        throw new ArgumentException("Independient Fuzzy Set '" + condition.Key + "' does not contain rule '" + condition.Value + "'");
                    }
                }
                else
                {
                    throw new ArgumentException("Fuzzy System '" + name + "' does not contain Independient Fuzzy Set '" + condition.Key + "'");
                }
            }
            rules[rule.Consequent().Key].Add(rule);
        }

        public Dictionary<string, Dictionary<string, float>> Fuzzify(Dictionary<string, float> inputs)
        {
            Dictionary<string, Dictionary<string, float>> fuzzyInputs = new Dictionary<string, Dictionary<string, float>>();

            foreach (KeyValuePair<string, FuzzyVariable> fuzzyVariable in independientSets)
            {
                if (inputs.ContainsKey(fuzzyVariable.Key))
                {
                    fuzzyInputs.Add(fuzzyVariable.Key, fuzzyVariable.Value.Fuzzification(inputs[fuzzyVariable.Key]));
                }
                else
                {
                    throw new ArgumentException("Inputs does not contain value for FuzzyVariable with name '" + fuzzyVariable.Key + "'");
                }
            }

            return fuzzyInputs;
        }

        public Dictionary<string, List<KeyValuePair<string, float>>> FuzzyOperator(Dictionary<string, Dictionary<string, float>> fuzzyInputs, string andOperatorMethod="min")
        {
            Dictionary<string, List<KeyValuePair<string, float>>> operator_results = new Dictionary<string, List<KeyValuePair<string, float>>>();
            List<KeyValuePair<string, float>> operator_result;
            KeyValuePair<string, float> result;
            List<float> mu_s;
            foreach (KeyValuePair<string, FuzzyVariable> set in dependientSets)
            {
                operator_result = new List<KeyValuePair<string, float>>();
                foreach (Rule rule in rules[set.Key])
                {
                    mu_s = new List<float>();
                    foreach (KeyValuePair<string, string> antecedent in rule.Antecedents())
                    {
                        mu_s.Add(fuzzyInputs[antecedent.Key][antecedent.Value]);
                    }
                    result = new KeyValuePair<string, float>(rule.Consequent().Value, AndOperator(mu_s, andOperatorMethod) * rule.Weight()); // Multiply by weight as the process of implication method
                    operator_result.Add(result);
                }
                operator_results.Add(set.Key, operator_result);
            }
            return operator_results;
        }

        public Dictionary<string, float> Inference(Dictionary<string, List<KeyValuePair<string, float>>> outputs, string inferenceMethod = "last_of_maxima")
        {
            Dictionary<string, float> defuzziedOutputs = new Dictionary<string, float>();
            if (inferenceMethod == "last_of_maxima")
            {
                string maxima_fuzzy_set;
                float maxima_mu;
                float maxima_intersection;
                foreach (KeyValuePair<string, FuzzyVariable> set in dependientSets)
                {
                    maxima_fuzzy_set = "";
                    maxima_mu = 0;
                    maxima_intersection = 0;
                    foreach (KeyValuePair<string, float> output in outputs[set.Key])
                    {
                        if (maxima_mu < output.Value)
                        {
                            maxima_fuzzy_set = output.Key;
                            maxima_mu = output.Value;
                            maxima_intersection = dependientSets[set.Key].LastIntersection(maxima_fuzzy_set, maxima_mu);
                        }
                        else if( maxima_mu == output.Value && output.Value > 0)
                        {
                            if(maxima_intersection < dependientSets[set.Key].LastIntersection(output.Key, output.Value))
                            {
                                maxima_fuzzy_set = output.Key;
                                maxima_mu = output.Value;
                                maxima_intersection = dependientSets[set.Key].LastIntersection(maxima_fuzzy_set, maxima_mu);
                            }
                        }
                    }
                    defuzziedOutputs.Add(set.Key, maxima_intersection);
                }
            }
            else if (inferenceMethod == "first_of_maxima")
            {
                string maxima_fuzzy_set;
                float maxima_mu;
                float maxima_intersection;
                foreach (KeyValuePair<string, FuzzyVariable> set in dependientSets)
                {
                    maxima_fuzzy_set = "";
                    maxima_mu = 0;
                    maxima_intersection = 0;
                    foreach (KeyValuePair<string, float> output in outputs[set.Key])
                    {
                        if (maxima_mu < output.Value)
                        {
                            maxima_fuzzy_set = output.Key;
                            maxima_mu = output.Value;
                            maxima_intersection = dependientSets[set.Key].LastIntersection(maxima_fuzzy_set, maxima_mu);
                        }
                        else if (maxima_mu == output.Value && output.Value > 0)
                        {
                            if (maxima_intersection < dependientSets[set.Key].LastIntersection(output.Key, output.Value))
                            {
                                maxima_fuzzy_set = output.Key;
                                maxima_mu = output.Value;
                                maxima_intersection = dependientSets[set.Key].LastIntersection(maxima_fuzzy_set, maxima_mu);
                            }
                        }
                    }
                    defuzziedOutputs.Add(set.Key, maxima_intersection);
                }
            }
            return defuzziedOutputs;
        }

        public Dictionary<string, float> FuzzyInferenceSystem(Dictionary<string, float> crispInputs, string andOperatorMethod = "min", string inferenceMethod = "last_of_maxima")
        {
            Dictionary<string, Dictionary<string, float>> fuzzyInputs = Fuzzify(crispInputs);

            Dictionary<string, List<KeyValuePair<string, float>>> fuzzyOutputs = FuzzyOperator(fuzzyInputs, andOperatorMethod);

            Dictionary<string, float> crispOutputs = Inference(fuzzyOutputs, inferenceMethod);

            return crispOutputs;
        }

        public float AndOperator(List<float> values, string method = "min")
        {
            if (values.Count > 0)
            {
                if (method == "min")
                {
                    float min = 1;
                    foreach (float value in values)
                    {
                        if (min > value)
                            min = value;
                    }
                    return min;
                }
                else if (method == "prod")
                {
                    float result = 1;
                    foreach (float value in values)
                    {
                        result *= value;
                    }
                    return result;
                }
                else
                {
                    throw new ArgumentException("Function '" + method + "' does not exist as an And Operator");
                }
            }
            return 0;
        }

        public string Name()
        {
            return name;
        }
    }

    public class Rule
    {
        Dictionary<string, string> antecedents;
        KeyValuePair<string, string> consequent;
        float weight;

        public Rule(Dictionary<string, string> antecedents, KeyValuePair<string, string> consequent, float weight = 1f)
        {
            this.antecedents = antecedents;
            this.consequent = consequent;
            this.weight = weight;
        }

        public KeyValuePair<string, string> Consequent()
        {
            return consequent;
        }

        public Dictionary<string, string> Antecedents()
        {
            return antecedents;
        }

        public float Weight()
        {
            return weight;
        }
    }

    public class FuzzyVariable
    {
        protected Dictionary<string, FuzzySet> fuzzySets;
        string name;

        public FuzzyVariable(string name)
        {
            fuzzySets = new Dictionary<string, FuzzySet>();
            this.name = name;
        }

        public string Name()
        {
            return name;
        }

        public bool FuzzySetExists(string rule)
        {
            return fuzzySets.ContainsKey(rule);
        }

        public void AddFuzzySet(FuzzySet rule)
        {
            if (fuzzySets.ContainsKey(rule.Name()))
            {
                throw new ArgumentException("FuzzySet '" + rule.Name() + "' already added");
            }
            fuzzySets.Add(rule.Name(), rule);
        }

        public Dictionary<string, float> Fuzzification(float value)
        {
            Dictionary<string, float> fuzziedSets = new Dictionary<string, float>();

            foreach (KeyValuePair<string, FuzzySet> fuzzySet in fuzzySets)
            {
                fuzziedSets.Add(fuzzySet.Key, fuzzySet.Value.MembershipFunction(value));
            }

            return fuzziedSets;
        }

        public float FirstIntersection(string set, float membership)
        {
            return fuzzySets[set].FirstIntersection(membership);
        }

        public float LastIntersection(string set, float membership)
        {
            return fuzzySets[set].LastIntersection(membership);
        }
    }

    public class FuzzySet
    {
        string name;
        protected FuzzySet(string name)
        {
            this.name = name;
        }
        public virtual bool IndicatorFunction(float value)
        {
            return false;
        }

        public virtual float MembershipFunction(float value)
        {
            return 0;
        }

        public virtual float FirstIntersection(float value)
        {
            return 0;
        }

        public virtual float LastIntersection(float value)
        {
            return 0;
        }

        public string Name()
        {
            return name;
        }
    }

    public class DiagonalLineFuzzySet : FuzzySet
    {

        float zero;
        float one;
        float m;
        public DiagonalLineFuzzySet(string name, float zero, float one) : base(name)
        {
            if (zero != one)
            {
                this.zero = zero;
                this.one = one;
                m = 1 / (one - zero);
            }
            else
            {
                throw new ArgumentException("Parameter 'zero' must be diferent than parameter 'one'");
            }
        }

        public override bool IndicatorFunction(float value)
        {
            if (zero < one)
            {
                if (value <= zero)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (value >= zero)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public override float MembershipFunction(float value)
        {
            if (zero < one)
            {
                if (value <= zero)
                {
                    return 0;
                }
                else if (value >= one)
                {
                    return 1;
                }
                else
                {
                    return m * (value - zero);
                }
            }
            else
            {
                if (value >= zero)
                {
                    return 0;
                }
                else if (value <= one)
                {
                    return 1;
                }
                else
                {
                    return 1 + m * (value - one);
                }
            }
        }

        public override float FirstIntersection(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
            float intersection;
            if (zero < one)
            {
                intersection = value / m + zero;
            }
            else
            {
                intersection = (value - 1) / m + one;
            }
            return intersection;
        }

        public override float LastIntersection(float value)
        {
            return FirstIntersection(value);
        }
    }

    public class TriangularFuzzySet : FuzzySet
    {
        float min_zero;
        float min_m;
        float one;
        float max_zero;
        float max_m;
        public TriangularFuzzySet(string name, float min_zero, float one, float max_zero) : base(name)
        {
            if (min_zero < one && one < max_zero)
            {
                this.min_zero = min_zero;
                if (min_zero != one)
                    min_m = 1 / (one - min_zero); // funcion de la pendiente, 1 porque 1-0 = 1 
                this.one = one;
                this.max_zero = max_zero;
                if (max_zero != one)
                    max_m = 1 / (one - max_zero); // funcion de la pendiente, 1 porque 1-0 = 1 
            }
            else
            {
                throw new ArgumentException("Parameter 'min_zero' must be less than parameter 'one', and parameter 'one' must be less than parameter 'max_zero'");
            }
        }

        public override bool IndicatorFunction(float value)
        {
            if (value <= min_zero || value >= max_zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override float MembershipFunction(float value)
        {
            if (value <= min_zero || value >= max_zero)
            {
                return 0;
            }
            if (value < one)
            {
                return min_m * (value - min_zero);
            }
            else
            {
                return 1 + max_m * (value - one);
            }
        }

        public override float FirstIntersection(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
            float intersection = value / min_m + min_zero;
            return intersection;
        }

        public override float LastIntersection(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
            float intersection = (value - 1) / max_m + one;
            return intersection;
        }
    }

    public class TrapezoidFuzzySet : FuzzySet
    {
        float min_zero;
        float min_m;
        float min_one;
        float max_one;
        float max_zero;
        float max_m;
        public TrapezoidFuzzySet(string name, float min_zero, float min_one, float max_one, float max_zero) : base(name)
        {
            if (min_zero < min_one && min_one < max_one && max_one < max_zero)
            {
                this.min_zero = min_zero;
                this.min_one = min_one;
                min_m = 1 / (min_one - min_zero); // funcion de la pendiente, 1 porque 1-0 = 1
                this.max_one = max_one;
                this.max_zero = max_zero;
                max_m = -1 / (max_one - max_zero);
            }
            else
            {
                throw new ArgumentException("Parameter 'min_zero' must be less than parameter 'min_one', parameter 'min_one' must be less than parameter 'max_one' and parameter 'max_one' must be less than parameter 'max_zero'");
            }
        }

        public override bool IndicatorFunction(float value)
        {
            if (value <= min_zero || value >= max_zero)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override float MembershipFunction(float value)
        {
            if (value <= min_zero || value >= max_zero)
            {
                return 0;
            }
            if (min_one <= value && value <= max_one)
            {
                return 1;
            }
            if (value < min_one)
            {
                return min_m * (value - min_zero);
            }
            else
            {
                return 1 + max_m * (value - max_one);
            }
        }

        public override float FirstIntersection(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
            float intersection = value / min_m + min_zero;
            return intersection;
        }

        public override float LastIntersection(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
            float intersection = (value - 1) / max_m + max_one;
            return intersection;
        }
    }
}