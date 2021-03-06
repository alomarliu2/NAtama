﻿using LitJson;
using Model.DBF;
using NLNetwork;
using System.Collections;
using UIFrameWork;
using UnityEngine;

/**=============================================================
 * <summary> 尋找對手 </summary>
 * 
 * <author>Neymar Liu</author>
 * <date>$time$</date>
 * 
 * Copyright (c) 2014 All Rights Reserved
 =============================================================*/

public class UIFindTargetController : UIController
{
	private static UIFindTargetController _instance = null;
   
    [SerializeField]
    UILabel _lbRemains = null;
    [SerializeField]
    UILabel _lbBet = null;

    [SerializeField]
    GameObject _find = null;
    [SerializeField]
    UILabel _lb1PName = null;
    [SerializeField]
    GameObject _ui1P = null;
    [SerializeField]
    GameObject _ui2P = null;
    [SerializeField]
    UILabel _lb2PName = null;
    [SerializeField]
    GameObject _effectPanel = null;
    [SerializeField]
    GameObject _btnCancel = null;

    bool _serverReady = false;

    const float _maxTime = 5.0f;

    float _remainTime = -1;

    bool _play = false;

    /// <summary>注金</summary>
    int _bet = 0;

	/**=============================================
	 * 取得 Singleton
	 * ===========================================*/
	public static UIFindTargetController instance
	{
		get
		{
			if(_instance == null)
				_instance = UIManager.instance.GetUI<UIFindTargetController>();

			return _instance;
		}
	}

	override public void Open(params object[] values)
	{		
		base.Open();

        if (values.Length <= 0)
            _bet = 0;
        else
            _bet = (int)values[1];

        UpdateUI();

        StartCoroutine(FindTarget());
	}
        	
	override public void Close()
	{		
		// do Something
		
		base.Close();
	}
    
	/**================================
	 * <summary> 更新畫面 </summary>
	 *===============================*/
    private void UpdateUI()
    {
        _lbBet.text = "0";
        _lb1PName.text = Character.ins.playName;
        
        SnatchStageLib obj = DBFManager.snatchStageLib.Data(Character.ins.charID) as SnatchStageLib;

        // 更新角色名稱及圖示
        if (null != obj)
        {
            string uiName = string.Format("Npc{0:000}", obj.GUID);
            UISprite uiNpc = _ui1P.GetComponent<UISprite>();
            uiNpc.spriteName = uiName;

            // 更換NPC圖片
            uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.Free;
            uiNpc.MakePixelPerfect();
            uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
            uiNpc.SetDimensions(460, 200);
        }
    }

	/**================================
	 * <summary> 更新 </summary>
	 *===============================*/
    public void FixedUpdate()
    {
        if (_remainTime >= 0)
        {
            _remainTime -= Time.deltaTime;
            _lbRemains.text = _remainTime.ToString("0");
        }
    }
    
	/**================================
	 * <summary> 更新畫面 </summary>
	 *===============================*/
    IEnumerator FindTarget()
    {
        _remainTime = _maxTime;
        float num = Random.Range(2, 4);
        
        yield return new WaitForSeconds(num);

        // 找到了
        Found();

        // 等2秒
        yield return new WaitForSeconds(2f);

        if (_bet > 0)
        {
            GameObject prefab = Resources.Load<GameObject>("uiCoin");
            GameObject coin = null;
            Vector3[] v = iTweenPath.GetPath("Coin1");
            Vector3[] v2 = iTweenPath.GetPath("Coin2");
            Vector3[] v3 = iTweenPath.GetPath("Coin3");
            Vector3[] v4 = iTweenPath.GetPath("Coin4");
            Vector3[] tempv = null;

            NGUITools.SetActiveSelf(_lbBet.gameObject, true);

            for (int i = 0; i < 10; i++)
            {
                coin = NGUITools.AddChild(_effectPanel, prefab);

                tempv = (i % 2 == 0) ? v : v2;
                iTween.PutOnPath(coin, tempv, 0f);
                iTween.MoveTo(coin, iTween.Hash("name", "abc", "time", 1.5, "path", tempv, "easetype", "easeInOutSine",
                                                "delay", 0.1f * i,
                                                "oncomplete", "OnComplete",
                                                "oncompletetarget", gameObject,
                                                "oncompleteparams", coin));

                coin = NGUITools.AddChild(_effectPanel, prefab);

                tempv = (i % 2 == 0) ? v3 : v4;
                iTween.PutOnPath(coin, tempv, 0f);
                iTween.MoveTo(coin, iTween.Hash("name", "abc", "time", 1.5, "path", tempv, "easetype", "easeInOutSine",
                                                "delay", 0.1f * i,
                                                "oncomplete", "OnComplete",
                                                "oncompletetarget", gameObject,
                                                "oncompleteparams", coin));
            }

            // 等2秒
            yield return new WaitForSeconds(3f);
        }        

        while (!_serverReady)
            yield return new WaitForSeconds(0.1f);
            
        Close();
        // 轉場
        SceneLoader.ins.LoadLevel(Scenes.Battle);
    }

    public void Battle()
    {
        _serverReady = true;
    }
    
	/**================================
	 * <summary>  找到對手了  </summary>
	 *===============================*/
    private void Found()
    {
        _serverReady = false;

        NGUITools.SetActiveSelf(_btnCancel, false);
        NGUITools.SetActiveSelf(_find, false);
        NGUITools.SetActiveSelf(_lb2PName.gameObject, true);
        NGUITools.SetActiveSelf(_ui2P, true);
        
        JsonData jd = PlayerPrefManager.instance.GetBattleInfo();
        int mode = int.Parse(jd["mode"].ToString());
        int stageID = int.Parse(jd["stageID"].ToString());
        int npcID = int.Parse(jd["npcID"].ToString());
        
        // 改變 2PIcon
        Change2PIcon();

        // 一般模式把ID清0
        if (mode == (int)Enum_BattleMode.ModeA)
            npcID = 0;

        // 傳送指令
        NetworkLib.instance.SendEnterStage(Character.ins.uid, stageID, npcID, mode);
        
        if (mode == (int)Enum_BattleMode.ModeB)
        {
            Character.ins.challengeCount--;
            // 重新rand一個角色
            npcID = Character.ins.RandStageNpc(stageID);

            Character.ins.everyDayNpcs[stageID - 1] = npcID;
            // 寫入暫存
            PlayerPrefManager.instance.SetEveryDayNPC(Character.ins.everyDayNpcs);
        }

    }
    
	/**================================
	 * <summary> 更換2p圖示 </summary>
	 *===============================*/
    void Change2PIcon()
    {        
        JsonData jd = PlayerPrefManager.instance.GetBattleInfo();
        int mode = int.Parse(jd["mode"].ToString());
        int stageID = int.Parse(jd["stageID"].ToString());
        int npcID = int.Parse(jd["npcID"].ToString());

        switch((Enum_BattleMode)mode)
        {
            case Enum_BattleMode.ModeB:
            case Enum_BattleMode.ModeC:
                {
                    ChallengeNpcLib obj = DBFManager.challengeNpcLib.Data(npcID) as ChallengeNpcLib;

                    if(null != obj)
                    {                        
                        _lb2PName.text = obj.Name;
                        string uiName = string.Format("Npc{0:000}", obj.GUID);
                        
                        UISprite uiNpc = _ui2P.GetComponent<UISprite>();
                        uiNpc.spriteName = uiName;

                        // 更換NPC圖片
                        uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.Free;
                        uiNpc.MakePixelPerfect();
                        uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
                        uiNpc.SetDimensions(460, 200);
                    }
                }
                break;
            case Enum_BattleMode.ModeA:
            case Enum_BattleMode.ModeD:
                {                    
                    SnatchStageLib obj = DBFManager.snatchStageLib.Data(npcID) as SnatchStageLib;

                    // 更新角色名稱及圖示
                    if (null != obj)
                    {
                        _lb2PName.text = obj.NAME;
                        string uiName = string.Format("Npc{0:000}", obj.GUID);
                        UISprite uiNpc = _ui2P.GetComponent<UISprite>();
                        uiNpc.spriteName = uiName;

                        // 更換NPC圖片
                        uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.Free;
                        uiNpc.MakePixelPerfect();
                        uiNpc.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
                        uiNpc.SetDimensions(460, 200);
                    }
                }
                break;
        }
    }

    void OnComplete(GameObject obj)
    {
        Destroy(obj);

        if(!_play)
        {
            StartCoroutine(PlayCoin());
            _play = true;
        }
    }
    
	/**================================
	 * <summary> 播放注金增加動態 </summary>
	 *===============================*/
    IEnumerator PlayCoin()
    {
        // 分10段顯示
        int bet = _bet * 2 / 10;

        for(int i = 0; i < 10; ++i)
        {
            _lbBet.text = (bet * (i + 1)).ToString();
            _lbBet.animation.Play();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }
}
