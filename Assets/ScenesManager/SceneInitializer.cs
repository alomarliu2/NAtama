﻿using UnityEngine;
using System.Collections;

public class SceneInitializer : MonoBehaviour 
{	
	IEnumerator Start () 
	{		
		yield return new WaitForSeconds(3);
		
		SceneManager.ins.LoadOwnScene();
	}
}