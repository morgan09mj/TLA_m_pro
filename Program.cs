using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace TLA_Lib
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
}