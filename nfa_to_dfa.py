import json
import sys

def find_lambda_transitions(state):
result = []
if nfa_transitions.get((state, ''):
)) :
for lambda_transition in nfa_transitions[(state, ''):
)] :
if lambda_transition not in result:
result.append(lambda_transition)
for item in result: 
    lambda_states = find_lambda_transitions(item) 
    for lambda_state in lambda_states: 
        result.append(lambda_state) 

return result
def transition_states(state, alphabet, result):
    if nfa_transitions.get((state, alphabet)):
for transition_state in nfa_transitions[(state, alphabet)]:
if transition_state not in result:
result.append(transition_state)
lambda_transitions = result.copy() 
for s in lambda_transitions: 
    lambda_states = find_lambda_transitions(s) 
    for lambda_state in lambda_states: 
        if lambda_state not in result: 
            result.append(lambda_state) 

lambda_states = find_lambda_transitions(state) 
for item in lambda_states: 
    transition_states(item, alphabet, result) 

return result
def find_new_states(current_state):
    for nfa_alphabet in nfa_alphabets:
resulting_states = []
for state in current_state:
result = transition_states(state, nfa_alphabet, [])
for item in result:
if item not in resulting_states:
resulting_states.append(item)
resulting_states = sorted(resulting_states) 

		
    dfa_transitions[(tuple(current_state), nfa_alphabet)] = resulting_states 
    if resulting_states not in dfa_states: 
        dfa_states.append(resulting_states) 

		
        if len(resulting_states) > 0: 
            new_states.append(resulting_states)
            def nfa_initialization(json_path):
nfa = json.load(open(json_path))
nfa_states = [state[1: -1] for state in nfa['states'][1: -1].split(',')] 
nfa_alphabets = [alphabet[1: -1] for alphabet in nfa['input_symbols'][1: -1].split(',')] 

		
nfa_transitions = {} 
for key in nfa['transitions']: 
    for k in nfa['transitions'][key].keys(): 
        result_states = [s[1: -1] for s in nfa['transitions'][key][k][1: -1].split(',')] 
        for result_state in result_states: 
            if (key, k) not in nfa_transitions: 
                nfa_transitions[(key, k)] = [result_state] 
			else: 
                nfa_transitions[(key, k)].append(result_state) 

		
nfa_initial_state = nfa['initial_state'] 
nfa_final_states = [state[1: -1] for state in nfa['final_states'][1: -1].split(',')] 

		
return nfa_states, nfa_alphabets, nfa_transitions, nfa_initial_state, nfa_final_states