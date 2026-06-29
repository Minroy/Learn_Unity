///--------------------------------------------///
///-----MADE WITH: UNODE VISUAL SCRIPTING-----///
///------------------------------------------///
#pragma warning disable
using UnityEngine;
using System.Collections.Generic;

public class Test : MonoBehaviour {	
	private MaxyGames.StateMachines.StateMachine m_FSM;
	private MaxyGames.StateMachines.State m_state_State_1;
	private MaxyGames.StateMachines.State m_state_State_2;
	public string icy = "";
	public List<int> TestList = new List<int>();
	
	private void Awake() {
		m_FSM = new MaxyGames.StateMachines.StateMachine();
		m_state_State_1 = new MaxyGames.StateMachines.State() { 
			onEnter = () => {
				Debug.Log("State 1");
			}, 
			onUpdate = () => {
				if(Input.GetKeyDown(KeyCode.Space)) {
					m_FSM.ChangeState(m_state_State_2);
				}
			} };
		m_state_State_1.FSM = m_FSM;
		m_state_State_2 = new MaxyGames.StateMachines.State() { 
			onUpdate = () => {
				if(Input.GetKeyDown(KeyCode.Space)) {
					m_FSM.ChangeState(m_state_State_1);
				}
			} };
		m_state_State_2.FSM = m_FSM;
		m_FSM.ChangeState(m_state_State_1);
	}
	
	private void Update() {
		hh();
		m_FSM.Tick();
	}
	
	public void hh() {
		if((icy == null)) {
			if(false) {
				foreach(int loopValue in TestList) {}
			}
		}
		if(false) {}
	}
}

