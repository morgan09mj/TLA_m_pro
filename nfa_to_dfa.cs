using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TLA_Lib;
namespace nfa_to_dfa_1 
{
    public class NFAConverter
    {
        public static DFA ConvertNFAToDFA(NFA nfa)
        {
            //Create new list of states for final DFA
            List<string> allStates = new List<string>();

            //Add input NFA's initial state to previous list
            allStates.Add(nfa.InitialState.state_ID);

            //Create new DFA object for final DFA
            DFA new_automata = new DFA(allStates, nfa.InputSymbols.inputs, nfa.InitialState.state_ID, new List<string>());

            //Create list for the states which their transitions have not been determined 
            List<NewState> new_states = new List<NewState>();

            //Add result DFA's initial state to previous list to determine it's transitions
            new_states.Add(new_automata.InitialState);

            //Create Queue to determine which states of input NFA are in result DFA states 
            Queue<State> included_in_new_state = new Queue<State>();

            NewState tmp = new_automata.InitialState;

            //Add input NFA's initial state to the list of NFA states of result DFA state & Enqueue initial state 
            tmp.includedNFAStates.Add(nfa.InitialState);
            included_in_new_state.Enqueue(nfa.InitialState);

            //ino tarif konim
            List<State> eachIncludedStateDes = new List<State>();

            //List of states that we go through lambda 
            List<State> new_eachIncludedStateDes = new List<State>();

            //While loop for fill previous list 
            while (included_in_new_state.Count > 0)
            {
                State tmp_1 = included_in_new_state.Dequeue();
                new_eachIncludedStateDes.Add(tmp_1);
                if (tmp_1.transitions.ContainsKey(""))
                {
                    for (int k = 0; k < tmp_1.transitions[""].Count; k++)
                    {
                        if (!new_eachIncludedStateDes.Contains(tmp_1.transitions[""][k]) && !included_in_new_state.Contains(tmp_1.transitions[""][k]))
                        {
                            included_in_new_state.Enqueue(tmp_1.transitions[""][k]);
                        }
                    }
                }
            }
            //Order previous filled list by state's IDs(name)
            new_eachIncludedStateDes = new_eachIncludedStateDes.OrderBy(x => x.state_ID.Replace("q", "")).ToList();
            
            //Fill the list of NFA states in new DFA state that we constructed
            for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                tmp.includedNFAStates.Add(new_eachIncludedStateDes[i]);

            //Concat the NFA states IDs that are included in new DFA state
            tmp.string_nfa_states = nfa.InitialState.state_ID;

            //Repeat this do while loop until we check all result DFA state's transitions
            do
            {
                
                if (new_automata.InputSymbols.inputs.Count == 2)
                {
                    eachIncludedStateDes.Clear();
                    //Add each NFA state's transitions for first input symbol that is included in new DFA state 
                    for (int j = 0; j < new_states[0].includedNFAStates.Count; j++)
                    {
                        if (new_states[0].includedNFAStates[j].transitions.ContainsKey(new_automata.InputSymbols.inputs[0]))
                        {
                            for (int k = 0; k < new_states[0].includedNFAStates[j].transitions[new_automata.InputSymbols.inputs[0]].Count; k++)
                                eachIncludedStateDes.Add(new_states[0].includedNFAStates[j].transitions[new_automata.InputSymbols.inputs[0]][k]);
                        }
                    }
                    //Remove duplicated transitions
                    eachIncludedStateDes = eachIncludedStateDes.Distinct().ToList();

                    //Clear queue to handle lambda transitions
                    included_in_new_state.Clear();

                    //Add the destination states of NFA states that are included in DFA state that has lambda transitions to other states 
                    for (int j = 0; j < eachIncludedStateDes.Count; j++)
                        included_in_new_state.Enqueue(eachIncludedStateDes[j]);
                    new_eachIncludedStateDes.Clear();
                    while (included_in_new_state.Count > 0)
                    {
                        State tmp_2 = included_in_new_state.Dequeue();
                        new_eachIncludedStateDes.Add(tmp_2);
                        if (tmp_2.transitions.ContainsKey(""))
                        {
                            for (int k = 0; k < tmp_2.transitions[""].Count; k++)
                            {
                                if (!new_eachIncludedStateDes.Contains(tmp_2.transitions[""][k]) && !included_in_new_state.Contains(tmp_2.transitions[""][k]))
                                    included_in_new_state.Enqueue(tmp_2.transitions[""][k]);
                            }
                        }
                    }

                    //Order NFA states that are included in final DFA by state ID
                    new_eachIncludedStateDes = new_eachIncludedStateDes.OrderBy(x => x.state_ID.Replace("q", "")).ToList();

                    //Construct the string of previous NFA states IDs
                    string temp = "";
                    for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                        temp += new_eachIncludedStateDes[i].state_ID;

                    //Check whether new DFA state is empty 
                    //Define it as a TRAP state
                    if (new_eachIncludedStateDes.Count == 0)
                    {
                        bool existance = false;
                        int find_number = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].state_ID == "TRAP")
                            {
                                existance = true;
                                find_number = i;
                                break;
                            }
                        }
                        //check whether we have this TRAP if yes -->Update it's transitions if no --> Construct it
                        if (!existance)
                        {
                            NewState new_St = new NewState("TRAP");
                            new_St.includedNFAStates = new List<State>();
                            new_St.string_nfa_states = "";
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_St);
                            new_St.transitions[new_automata.InputSymbols.inputs[0]] = new_St;
                            new_St.transitions[new_automata.InputSymbols.inputs[1]] = new_St;
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_automata.States[find_number]);
                        }
                    }
                    else
                    {
                        //check whether new DFA state is not empty (it's not also a TRAP state)
                        bool check = false;
                        int number_StateDFA = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].string_nfa_states == temp)
                            {
                                check = true;
                                number_StateDFA = i;
                                break;
                            }
                        }
                        //Check whether we have constructed this DFA state if yes --> Update the transitions if no --> Construct DFA state and update it's transitions
                        if (!check)
                        {
                            NewState new_St = new NewState(temp);
                            for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                                new_St.includedNFAStates.Add(new_eachIncludedStateDes[i]);
                            new_St.string_nfa_states = temp;
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_St);
                            new_states.Add(new_St);
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_automata.States[number_StateDFA]);
                        }
                    }
                    eachIncludedStateDes.Clear();

                    //Add each NFA state's transitions for second input symbol that is included in new DFA state 
                    //Repeat previous operations for second input symbol
                    for (int j = 0; j < new_states[0].includedNFAStates.Count; j++)
                    {
                        if (new_states[0].includedNFAStates[j].transitions.ContainsKey(new_automata.InputSymbols.inputs[1]))
                            eachIncludedStateDes = eachIncludedStateDes.Concat(new_states[0].includedNFAStates[j].transitions[new_automata.InputSymbols.inputs[1]]).ToList();
                    }
                    eachIncludedStateDes = eachIncludedStateDes.Distinct().ToList();
                    included_in_new_state.Clear();
                    for (int j = 0; j < eachIncludedStateDes.Count; j++)
                        included_in_new_state.Enqueue(eachIncludedStateDes[j]);

                    new_eachIncludedStateDes.Clear();
                    while (included_in_new_state.Count > 0)
                    {
                        State tmp_3 = included_in_new_state.Dequeue();
                        new_eachIncludedStateDes.Add(tmp_3);
                        if (tmp_3.transitions.ContainsKey(""))
                        {
                            for (int k = 0; k < tmp_3.transitions[""].Count; k++)
                            {
                                if (!new_eachIncludedStateDes.Contains(tmp_3.transitions[""][k]) && !included_in_new_state.Contains(tmp_3.transitions[""][k]))
                                {
                                    included_in_new_state.Enqueue(tmp_3.transitions[""][k]);
                                }
                            }
                        }
                    }
                    new_eachIncludedStateDes = new_eachIncludedStateDes.OrderBy(x => x.state_ID.Replace("q", "")).ToList();
                    temp = "";
                    for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                        temp += new_eachIncludedStateDes[i].state_ID;

                    if (new_eachIncludedStateDes.Count == 0)
                    {
                        bool existance = false;
                        int find_number = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].state_ID == "TRAP")
                            {
                                existance = true;
                                find_number = i;
                                break;
                            }
                        }
                        if (!existance)
                        {
                            NewState new_St = new NewState("TRAP");
                            new_St.includedNFAStates = new List<State>();
                            new_St.string_nfa_states = "";
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[1], new_St);
                            new_St.transitions[new_automata.InputSymbols.inputs[0]] = new_St;
                            new_St.transitions[new_automata.InputSymbols.inputs[1]] = new_St;
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[1], new_automata.States[find_number]);
                        }
                    }
                    else
                    {
                        bool check = false;
                        int number_StateDFA = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].string_nfa_states == temp)
                            {
                                check = true;
                                number_StateDFA = i;
                                break;
                            }
                        }
                        if (!check)
                        {
                            NewState new_St = new NewState(temp);
                            for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                                new_St.includedNFAStates.Add(new_eachIncludedStateDes[i]);
                            new_St.string_nfa_states = temp;
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[1], new_St);
                            new_states.Add(new_St);
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[1], new_automata.States[number_StateDFA]);
                        }
                    }
                    new_states.RemoveAt(0);
                }

                //Check whether we have only one input symbols 
                //Repeat previous operations
                if (new_automata.InputSymbols.inputs.Count == 1)
                {
                    eachIncludedStateDes.Clear();
                    for (int j = 0; j < new_states[0].includedNFAStates.Count; j++)
                    {
                        if (new_states[0].includedNFAStates[j].transitions.ContainsKey(new_automata.InputSymbols.inputs[0]))
                            eachIncludedStateDes = eachIncludedStateDes.Concat(new_states[0].includedNFAStates[j].transitions[new_automata.InputSymbols.inputs[0]]).ToList();
                    }
                    eachIncludedStateDes = eachIncludedStateDes.Distinct().ToList();
                    included_in_new_state.Clear();
                    for (int j = 0; j < eachIncludedStateDes.Count; j++)
                        included_in_new_state.Enqueue(eachIncludedStateDes[j]);

                    new_eachIncludedStateDes.Clear();
                    while (included_in_new_state.Count > 0)
                    {
                        State tmp_4 = included_in_new_state.Dequeue();
                        new_eachIncludedStateDes.Add(tmp_4);
                        if (tmp_4.transitions.ContainsKey(""))
                        {
                            for (int k = 0; k < tmp_4.transitions[""].Count; k++)
                            {
                                if (!new_eachIncludedStateDes.Contains(tmp_4.transitions[""][k]) && !included_in_new_state.Contains(tmp_4.transitions[""][k]))
                                {
                                    included_in_new_state.Enqueue(tmp_4.transitions[""][k]);
                                }
                            }
                        }
                    }
                    new_eachIncludedStateDes = new_eachIncludedStateDes.OrderBy(x => x.state_ID.Replace("q", "")).ToList();
                    string temp = "";
                    for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                        temp += new_eachIncludedStateDes[i].state_ID;

                    if (new_eachIncludedStateDes.Count == 0)
                    {
                        bool existance = false;
                        int find_number = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].state_ID == "TRAP")
                            {
                                existance = true;
                                find_number = i;
                                break;
                            }
                        }
                        if (!existance)
                        {
                            NewState new_St = new NewState("TRAP");
                            new_St.includedNFAStates = new List<State>();
                            new_St.string_nfa_states = "";
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_St);
                            new_St.transitions[new_automata.InputSymbols.inputs[0]] = new_St;
                            new_St.transitions[new_automata.InputSymbols.inputs[1]] = new_St;
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_automata.States[find_number]);
                        }
                    }
                    else
                    {
                        bool check = false;
                        int number_StateDFA = -1;
                        for (int i = 0; i < new_automata.States.Count; i++)
                        {
                            if (new_automata.States[i].string_nfa_states == temp)
                            {
                                check = true;
                                number_StateDFA = i;
                                break;
                            }
                        }
                        if (!check)
                        {
                            NewState new_St = new NewState(temp);
                            for (int i = 0; i < new_eachIncludedStateDes.Count; i++)
                                new_St.includedNFAStates.Add(new_eachIncludedStateDes[i]);
                            new_St.string_nfa_states = temp;
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_St);
                            new_states.Add(new_St);
                            new_automata.States.Add(new_St);
                        }
                        else
                        {
                            new_states[0].transitions.Add(new_automata.InputSymbols.inputs[0], new_automata.States[number_StateDFA]);
                        }
                    }
                    new_states.RemoveAt(0);
                }

            } while (new_states.Count > 0);

            for (int i = 0; i < new_automata.States.Count; i++)
            {
                for (int j = 0; j < new_automata.States[i].includedNFAStates.Count; j++)
                {
                    if (nfa.FinalStates.Contains(new_automata.States[i].includedNFAStates[j]))
                    {
                        new_automata.FinalStates.Add(new_automata.States[i]);
                        break;
                    }
                }
            }
            return new_automata;
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

        public static void Main(string[] args)
        {
            var json_text = File.ReadAllText(@"C:/Users/win_10/TLA01-Projects/samples/phase1-sample/in/input1.json");
            NFA nfa = ConvertJsonToNFA(json_text);
            DFA dfa = ConvertNFAToDFA(nfa);
            ConvertFAToJson.ConvertDFAToJson(dfa, @"C:/Users/win_10/TLA01-Projects/samples/phase1-sample/in/outputsi.json");
        }
    }
}