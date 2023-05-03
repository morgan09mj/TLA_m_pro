using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Simplification_DFA
{
    public class ConvertFAToJson
    {
        public static void ConvertNFAToJson(NFAWithSingleFinalState nfa,string result_path)
        {
            string states = "{" + string.Join(',', nfa.States.Select(x => $"'{x.state_ID}'")) + "}";
            string final_states = "{" +"'"+nfa.FinalState.state_ID+"'"+ "}";
            string initial_state = nfa.InitialState.state_ID;
            string input_symbols = "{" + string.Join(',', nfa.InputSymbols.inputs.Select(x => $"'{x}'")) + "}";
            var trans = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> temp;
            for (int i = 0; i < nfa.States.Count; i++)
            {
                temp = new Dictionary<string, string>();
                foreach (var item in nfa.States[i].transitions)
                {
                    var q = "{" + string.Join(',', item.Value.Select(x => $"'{x.state_ID}'")) + "}";
                    temp.Add(item.Key, q);
                }
                trans.Add(nfa.States[i].state_ID, temp);
            }
           
            FA result = new FA() { states = states, final_states = final_states, initial_state = initial_state, input_symbols = input_symbols, transitions = trans };
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            json = Regex.Unescape(json);
            File.WriteAllText(result_path, json, Encoding.UTF8);

        }
        public static void ConvertDFAToJson(DFA dfa,string result_path)
        {
            string states = "{" + string.Join(',', dfa.States.Select(x => $"\u0027{x.state_ID}\u0027")) + "}";
            string final_states = "{" + string.Join(',', dfa.FinalStates.Select(x => $"'{x.state_ID}'")) + "}";
            string initial_state = dfa.InitialState.state_ID;
            string input_symbols = "{" + string.Join(',', dfa.InputSymbols.inputs.Select(x => $"'{x}'")) + "}";
            var trans = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> temp;
            foreach (var item in dfa.States)
            {
                temp = new Dictionary<string, string>();
                foreach (var x in item.transitions)
                    temp.Add(x.Key, x.Value.state_ID);
                trans.Add(item.state_ID, temp);
            }
            FA result = new FA() { states = states, final_states = final_states, initial_state = initial_state, input_symbols = input_symbols, transitions = trans };
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            json = Regex.Unescape(json);
            File.WriteAllText(result_path, json, Encoding.UTF8);

        }
    }
    public class DFA
    {
        public List<NewState> States;
        public InputSymbols InputSymbols;
        public List<NewState> FinalStates;
        public NewState InitialState = null;

        //Assign DFA fields after converting from string to objects
        public DFA(int x, List<NewState> allStates, InputSymbols allInputSymbols, NewState initialState, List<NewState> finalStates)
        {
            States = allStates;
            FinalStates = finalStates;
            this.InputSymbols = allInputSymbols;
            InitialState = initialState;
        }
        //Convert DFA fields from string to objects 
        public DFA(List<string> allStates, List<string> allInputSymbols, string initialState, List<string> finalStates)
        {
            States = new List<NewState>();
            FinalStates = new List<NewState>();
            InputSymbols inputSymbols = new InputSymbols();
            inputSymbols.setInputs(allInputSymbols);
            InputSymbols = inputSymbols;

            for (int i = 0; i < finalStates.Count; i++)
                FinalStates.Add(new NewState(finalStates[i]));

            for (int i = 0; i < allStates.Count; i++)
                States.Add(new NewState(allStates[i]));

            for (int i = 0; i < allStates.Count; i++)
                if (allStates[i].Equals(initialState))
                    InitialState = States[i];
        }
        //Convert DFA fields from string to objects 
        public DFA(List<NewState> states, NewState initial_state, List<NewState> final_states,List<string> input_symbols, Dictionary<string, Dictionary<string, string>> transitions)
        {
            States = states;
            InitialState = initial_state;
            FinalStates = final_states;
            InputSymbols = new InputSymbols();
            InputSymbols.setInputs(input_symbols);

            for (int i = 0; i < States.Count(); i++)
            {
                var transition = transitions[States[i].state_ID];
                States[i].transitions = new Dictionary<string, NewState>();
                foreach (var item in transition)
                {
                    int state_index = States.FindIndex(x => x.state_ID == item.Value);
                    States[i].transitions.Add(item.Key, States[state_index]);
                }
            }
        }
    }

    public class NewState
    {
        public bool Visit;
        public string state_ID;
        public string string_nfa_states = "";
        public List<State> includedNFAStates;
        public Dictionary<string, NewState> transitions;
        //Construct new dfa state 
        public NewState(string stateName)
        {
            state_ID = stateName;
            transitions = new Dictionary<string, NewState>();
            includedNFAStates = new List<State>();
        }
    }
    public class FA
    {
        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }
    }
    public partial class InputSymbols
    {
        public List<string> inputs;
        public void setInputs(List<string> transitionSygma) =>
            inputs = transitionSygma;
    }
    public class NFA
    {
        public List<State> States;
        public InputSymbols InputSymbols;
        public List<State> FinalStates;
        public State InitialState = null;

        //Assign NFA fields after converting from string to objects
        public NFA(List<State> states, State initial_state, List<State> final_states,List<string> input_symbols, Dictionary<string, Dictionary<string, string>> transitions, int x)
        {
            States = states;
            InitialState = initial_state;
            FinalStates = final_states;
            InputSymbols = new InputSymbols();
            InputSymbols.setInputs(input_symbols);

            for (int i = 0; i < States.Count(); i++)
            {
                var transition = transitions[States[i].state_ID];
                States[i].transitions = new Dictionary<string, List<State>>();
                foreach (var item in transition)
                {
                    int state_index = States.FindIndex(x => x.state_ID == item.Value);
                    List<State> temp_transition = new List<State>();
                    temp_transition.Add(States[state_index]);
                    States[i].transitions.Add(item.Key, temp_transition);
                }
            }
        }
        //Convert NFA fields from string to objects 
        public NFA(List<State> states, State initial_state, List<State> final_states, List<string> input_symbols, Dictionary<string, Dictionary<string, string>> transitions)
        {
            States = states;
            InitialState = initial_state;
            FinalStates = final_states;
            InputSymbols = new InputSymbols();
            InputSymbols.setInputs(input_symbols);

            for (int i = 0; i < States.Count(); i++)
            {
                var transition = transitions[States[i].state_ID];
                foreach (var item in transition)
                {
                    var transition_string = Regex.Replace(item.Value, @"[{}']", "").Split(",").ToList();
                    var transition_list = transition_string.Select(x => States[int.Parse(x.Substring(1))]).ToList();
                    States[i].transitions.Add(item.Key, transition_list);
                }
            }
        }
        //Convert NFA fields from string to objects 
        public NFA(List<string> allStates, List<string> allInputSymbols, string initialState, List<string> finalStates)
        {
            States = new List<State>();
            FinalStates = new List<State>();
            InputSymbols inputSymbols = new InputSymbols();
            inputSymbols.setInputs(allInputSymbols);
            InputSymbols = inputSymbols;

            for (int i = 0; i < finalStates.Count; i++)
            {

                FinalStates.Add(new State(finalStates[i]));
            }
            for (int i = 0; i < allStates.Count; i++)
                States.Add(new State(allStates[i]));

            for (int i = 0; i < allStates.Count; i++)
                if (allStates[i].Equals(initialState))
                    InitialState = States[i];

        }
    }
    public class NFAWithSingleFinalState
    {
        public List<State> States;
        public InputSymbols InputSymbols;
        public State FinalState;
        public State InitialState;
        //Construct new NFA with only one final state 
        public NFAWithSingleFinalState(List<State> allStates, List<string> allInputSymbols, State initialState, State finalState)
        {
            States = new List<State>();
            FinalState = finalState;
            InitialState = initialState;
            InputSymbols = new InputSymbols();
            InputSymbols.setInputs(allInputSymbols);
            States = allStates;
        }
    }
    public class State
    {
        public string state_ID;
        public Dictionary<string, List<State>> transitions;
        //Construct new NFA state 
        public State(string stateName)
        {
            transitions = new Dictionary<string, List<State>>();
            state_ID = stateName;
        }
    }
    public class ConvertJsonToFA
    {
        public static DFA ConvertJsonToDFA(string path)
        {   
            FA fa = JsonSerializer.Deserialize<FA>(path);
            var states = Regex.Replace(fa.states, @"[{}']", "").Split(",").ToList();
            var _input_symbols = Regex.Replace(fa.input_symbols, @"[{}']", "").Split(",").ToList();
            var final_states = Regex.Replace(fa.final_states, @"[{}']", "").Split(",").ToList();
            var transition = fa.transitions.First().Value;
            DFA dfa = null;
            if (!transition.First().Value.Contains('{'))
            {
                var _states = states.Select(x => new NewState(x)).ToList();
                List<NewState> _final_states = new List<NewState>();
                for (int i = 0; i < final_states.Count(); i++)
                    _final_states.Add(_states.Where(x => x.state_ID == final_states[i]).First());
                var _initial_state = _states.Where(x => x.state_ID == fa.initial_state).First();
                dfa = new DFA(_states, _initial_state, _final_states, _input_symbols, fa.transitions);
            }
            return dfa;
        }
        public static NFA ConvertJsonToNFA(string path)
        {
            FA fa = JsonSerializer.Deserialize<FA>(path);
            var states_string_list = Regex.Replace(fa.states, @"[{}']", "").Split(",").ToList();
            var _states = states_string_list.Select(x => new State(x)).ToList();
            var _input_symbols = Regex.Replace(fa.input_symbols, @"[{}']", "").Split(",").ToList();
            var final_state_string_list = Regex.Replace(fa.final_states, @"[{}']", "").Split(",").ToList();
            List<State> _final_states = new List<State>();
            for (int i = 0; i < final_state_string_list.Count(); i++)
                _final_states.Add(_states.Where(x => x.state_ID == final_state_string_list[i]).First());
            var _initial_state = _states.Where(x => x.state_ID == fa.initial_state).First();
            var transition = fa.transitions.First().Value;
            NFA nfa = null;
            if (transition.First().Value.Contains('{'))
                nfa = new NFA(_states, _initial_state, _final_states, _input_symbols, fa.transitions);
            return nfa;
        }
    }
    public class DFAStateReduction
    {
        //DFS recursive function 
        static void DFS_Visit_Recursive(DFA dfa, NewState n, List<NewState> dfs_visit)
        {
            n.Visit = true;
            for (int i = 0; i < dfa.InputSymbols.inputs.Count; i++)
                if (n.transitions[dfa.InputSymbols.inputs[i]].Visit == false)
                {
                    dfs_visit.Add(n.transitions[dfa.InputSymbols.inputs[i]]);
                    DFS_Visit_Recursive(dfa, n.transitions[dfa.InputSymbols.inputs[i]], dfs_visit);
                }
        }
        public static DFA SimplificationDFA(DFA dfa)
        {

            List<NewState> dfs_visit = new List<NewState>();
            dfs_visit.Add(dfa.InitialState);

            //First we go through vertexes by DFS to define non-reachable states
            DFS_Visit_Recursive(dfa, dfa.InitialState, dfs_visit);
            dfa.States = dfs_visit;
            List<NewState> new_final_state = new List<NewState>();
            //Remove non-reachable states from DFA
            for (int i = 0; i < dfa.FinalStates.Count; i++)
            {
                if (dfs_visit.Contains(dfa.FinalStates[i]))
                    new_final_state.Add(dfa.FinalStates[i]);
            }
            dfa.FinalStates = new_final_state;
            
            //Construct zero_equivalence by seperating final and non-final states
            List<NewState> final = new List<NewState>();
            List<NewState> nonfinal  = new List<NewState>();
            for (int i = 0; i < dfa.States.Count; i++)
            {
                if (dfa.FinalStates.Contains(dfa.States[i]))
                    final.Add(dfa.States[i]);
                else
                    nonfinal.Add(dfa.States[i]);
            }

            //Define list for k_equivalence to reach k+1_equivalence(next equivalence table)
            List<List<NewState>> equal_state_k = new List<List<NewState>>();
            equal_state_k.Add(final);
            equal_state_k.Add(nonfinal);

            //Do this while loop till k+1_equivalence table is as same as k_equivalence
            while (true)
            {
                //Create states list as k+1_equivalence table   
                List<List<NewState>> equal_state_k_next = new List<List<NewState>>();
                for (int i = 0; i < equal_state_k.Count; i++)
                {
                    //For states that were equal in k_equivalence table we check equivalency in k+1_equivalence table
                    //Create a dictionary for seperating states that are in the same list in k_equivalence table 
                    Dictionary<string, List<NewState>> product_equal_states = new Dictionary<string, List<NewState>>();
                    //for states that were equal in k_equivalence table do this for loop
                    for (int j = 0; j < equal_state_k[i].Count; j++)
                    {
                        string x = "";
                        //Check whether we have 2 input symbols 
                        if (dfa.InputSymbols.inputs.Count == 2)
                        {
                            //Check whether for each input symbol which eqivalence list we reach + add first state of the previous list to x(string)
                            for (int k = 0; k < equal_state_k.Count; k++)
                            {
                                if (equal_state_k[k].Contains(equal_state_k[i][j].transitions[dfa.InputSymbols.inputs[0]]))
                                {
                                    x += equal_state_k[k][0].state_ID;
                                    break;
                                }
                            }
                            for (int k = 0; k < equal_state_k.Count; k++)
                            {
                                if (equal_state_k[k].Contains(equal_state_k[i][j].transitions[dfa.InputSymbols.inputs[1]]))
                                {
                                    x += equal_state_k[k][0].state_ID;
                                    break;
                                }
                            }
                        }
                        else if (dfa.InputSymbols.inputs.Count == 1)
                        {
                            //Check whether we have only 1 input symbol 
                            //Repeat previous operations for one input symbol
                            for (int k = 0; k < equal_state_k.Count; k++)
                            {
                                if (equal_state_k[k].Contains(equal_state_k[i][j].transitions[dfa.InputSymbols.inputs[0]]))
                                {
                                    x += equal_state_k[k][0].state_ID;
                                    break;
                                }
                            }
                        }
                        //if two states go through same euivalence list in k_equivalence table they have the same euivalence list in k+1_equivalence 
                        if (product_equal_states.ContainsKey(x))
                            product_equal_states[x].Add(equal_state_k[i][j]);
                        else
                        {
                            List<NewState> temp = new List<NewState>();
                            temp.Add(equal_state_k[i][j]);
                            product_equal_states.Add(x, temp);
                        }
                    }
                    //Construct k+1_equivalence table
                    foreach (var tmp in product_equal_states.Values)
                        equal_state_k_next.Add(tmp);

                }
                //Chekck whether k+1_equivalence and k_equivalence are the same if yes --> quit while loop if no --> go through next equivalenc table
                if (equal_state_k_next.Count == equal_state_k.Count)
                    break;

                equal_state_k = equal_state_k_next;
            }
            //For each equivalency list in k_equivalence table order the states by state ID
            for (int i = 0; i < equal_state_k.Count; i++)
                equal_state_k[i] = equal_state_k[i].OrderBy(x => x.state_ID.Replace("q", "")).ToList();

            //For every list in k_equivalence table order the states in each list by the first state of the list
            equal_state_k = equal_state_k.OrderBy(x => x[0].state_ID.Replace("q", "")).ToList();
            
            //Construct new DFA by the result of equivalence table
            List<NewState> new_dfa_states = new List<NewState>();
            for (int i = 0; i < equal_state_k.Count; i++)
            {
                string x = "";
                for (int j = 0; j < equal_state_k[i].Count; j++)
                    x += equal_state_k[i][j].state_ID;

                new_dfa_states.Add(new NewState(x));
            }
            List<NewState> new_dfa_final_states = new List<NewState>();
            for (int i = 0; i < equal_state_k.Count; i++)
            {
                if (dfa.FinalStates.Contains(equal_state_k[i][0]))
                    new_dfa_final_states.Add(new_dfa_states[i]);
            }
            NewState new_dfa_initial_state = null;
            for (int i = 0; i < equal_state_k.Count; i++)
            {
                if (equal_state_k[i].Contains(dfa.InitialState))
                {
                    new_dfa_initial_state = new_dfa_states[i];
                    break;
                }
            }
            DFA new_dfa = new DFA(0, new_dfa_states, dfa.InputSymbols, new_dfa_initial_state, new_dfa_final_states);
            if (dfa.InputSymbols.inputs.Count == 2)
            {
                for (int i = 0; i < equal_state_k.Count; i++)
                {
                    for (int k = 0; k < equal_state_k.Count; k++)
                    {
                        if (equal_state_k[k].Contains(equal_state_k[i][0].transitions[dfa.InputSymbols.inputs[0]]))
                        {
                            new_dfa_states[i].transitions[dfa.InputSymbols.inputs[0]] = new_dfa_states[k];
                            break;
                        }
                    }
                    for (int k = 0; k < equal_state_k.Count; k++)
                    {
                        if (equal_state_k[k].Contains(equal_state_k[i][0].transitions[dfa.InputSymbols.inputs[1]]))
                        {
                            new_dfa_states[i].transitions[dfa.InputSymbols.inputs[1]] = new_dfa_states[k];
                            break;
                        }
                    }
                }
            }
            else if (dfa.InputSymbols.inputs.Count == 1)
            {
                //Check whether we have only 1 input symbol 
                for (int i = 0; i < equal_state_k.Count; i++)
                {
                    for (int k = 0; k < equal_state_k.Count; k++)
                    {
                        if (equal_state_k[k].Contains(equal_state_k[i][0].transitions[dfa.InputSymbols.inputs[0]]))
                        {
                            new_dfa_states[i].transitions[dfa.InputSymbols.inputs[0]] = new_dfa_states[k];
                            break;
                        }
                    }
                }
            }
            return new_dfa;
        }
        public static void Main(string[] args)
        {
            var json_text = File.ReadAllText(@"C:/Users/win_10/TLA01-Projects/samples/phase2-sample/in/input1.json");
            DFA dfa = ConvertJsonToFA.ConvertJsonToDFA(json_text);
            DFA result = SimplificationDFA(dfa);
            ConvertFAToJson.ConvertDFAToJson(result, @"C:/Users/win_10/TLA01-Projects/samples/phase2-sample/in/outputsi.json");
        }
    }
}