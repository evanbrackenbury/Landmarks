/*
    Copyright (C) 2010  Jason Laczko

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEngine.UI;

public class ExperimentTask : MonoBehaviour{

	protected GameObject avatar;
	protected HUD hud;

	protected GameObject experiment;
	protected dbLog log;
	protected Experiment manager;
	protected avatarLog avatarLog;

	protected long task_start;
	
	public bool skip = false;
	public bool canIncrementLists = true;

	public int interval = 0;

	public TaskList interruptTasks;
	public int interruptInterval = 0;
	public int repeatInterrupts = 9999;
	public int currentInterrupt = 0;
	[HideInInspector] public ExperimentTask pausedTasks;
	[HideInInspector] public TaskList parentTask;

	[HideInInspector] public Camera firstPersonCamera;
	[HideInInspector] public Camera overheadCamera;

	[HideInInspector] public Button debugButton;
	[HideInInspector] public Button actionButton;
	[HideInInspector] public bool actionButtonClicked = false;

	public static bool killCurrent = false;


	public void Awake () 
	{
		Debug.Log ("Starting Experiment Task");
	}

	public void Start () {
		


	}
	
	public virtual void startTask() {	
		avatar = GameObject.FindWithTag ("Player");
		avatarLog = avatar.GetComponent("avatarLog") as avatarLog; //jdstokes 2015
		hud = avatar.GetComponent("HUD") as HUD;
		experiment = GameObject.FindWithTag ("Experiment");
		manager = experiment.GetComponent("Experiment") as Experiment;
		firstPersonCamera = manager.playerCamera;
		overheadCamera = manager.overheadCamera;
		log = manager.dblog;

		debugButton = manager.debugButton.GetComponent<Button> ();
		actionButton = manager.actionButton.GetComponent<Button> ();

		// Start listening for debug skips
		debugButton.onClick.AddListener (onDebugClick);

		task_start = Experiment.Now();
		hud.ForceShowMessage ();
		//currentInterrupt = 0;        Not here since after an interuupt we will restart
		
		log.log("TASK_START\t" + name + "\t" + this.GetType().Name,1 );		
	}
	
	public virtual void TASK_START () {
	}	
	
	public virtual bool updateTask () {

		bool attemptInterupt = false;
		
		if ( interruptInterval > 0 && Experiment.Now() - task_start >= interruptInterval)  {
	        attemptInterupt = true;
	    }
	    
		if( Input.GetButtonDown ("Compass") ) {
			attemptInterupt = true;	
		}    
	    
		if(attemptInterupt && interruptTasks && currentInterrupt < repeatInterrupts && !interruptTasks.skip) 
		{	
			if (interruptTasks.skip) {
				log.log("INFO	skip interrupt	" + interruptTasks.name,1 );
			} 
			else 
			{
				Debug.Log(currentInterrupt);
	    		Debug.Log(repeatInterrupts);
	    
				log.log("INPUT_EVENT	interrupt	" + name,1 );
				//interruptTasks.pausedTasks = this;
				parentTask.pausedTasks = this;
				TASK_PAUSE();
				//endTask();
				currentInterrupt = currentInterrupt + 1;
				interruptTasks.startTask();
				parentTask.currentTask = interruptTasks;
	
			}
		}


		
		return false;
	}
	public virtual void endTask() {
		long duration = Experiment.Now() - task_start;
		currentInterrupt = 0;    //put here because of interrupts
		log.log("TASK_END\t" + name + "\t" + this.GetType().Name + "\t" + duration,1 );
	}
	public virtual void TASK_END () {
	}
	public virtual void TASK_PAUSE () {
	}

	public virtual bool OnControllerColliderHit(GameObject hit)  {
		return false;
	}
	
	public virtual void TASK_ROTATE (GameObject go, Vector3 hpr) {
	}	
	public virtual void TASK_POSITION (GameObject go, Vector3 hpr) {
	}	
	public virtual void TASK_SCALE (GameObject go, Vector3 scale) {
	}		
	public virtual void TASK_ADD(GameObject go, string txt) {
	}
	
	public virtual string currentString() {
		return "";
	}
	
	public virtual void incrementCurrent() {
	}

	// http://www.haslo.ch/blog/setproperty-and-getproperty-with-c-reflection/
	private object getProperty(object containingObject, string propertyName)
	{
	    return containingObject.GetType().InvokeMember(propertyName, BindingFlags.GetProperty, null, containingObject, null);
	}
	 
	private void setProperty(object containingObject, string propertyName, object newValue)
	{
	    containingObject.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, containingObject, new object[] { newValue });
	}

	public void onDebugClick ()
	{
		killCurrent = true;
	}

	public void OnActionClick()
	{
		actionButtonClicked = true;
	}

	public bool KillCurrent () 
	{
		killCurrent = false;
		Debug.Log ("ForceKilling " + this.name);
		return true;
	}


}