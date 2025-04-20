using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using static EventManager;
public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[SerializeField]
	private Text scoreText;

	[SerializeField]
	private GameObject scorePopupPrefab;

	private int score;

	[InspectorButton("OnButtonClicked")]
	public bool DeletePlayerPrefs;

	public GameObject ContinuePanel;
	public Action PlayerRespawn;
	public Action PlayerDied;
	public static bool isCantap=true;
	public string clickid;
    private StarkAdManager starkAdManager;
    IEnumerator DisableTap()
    {
		isCantap = false;
        yield return new WaitForSeconds(2.0f);
		isCantap = true;
    }
    public void ShowContinuePanel()
	{
        Time.timeScale = 0;
        ContinuePanel.SetActive(true);
	}
	public void HideContinuePanel()
	{
        Time.timeScale = 1;
        ContinuePanel.SetActive(false);
    }
	private void OnButtonClicked()
	{
		PlayerPrefs.DeleteAll();
		UnityEngine.Debug.Log("PlayerPrefs Deleted");
	}

	private void Awake()
	{
		Application.targetFrameRate = 60;
		GameManager.instance = this;
	}

	private void Start()
	{
		this.score = 0;
		this.scoreText.text = this.score.ToString();
	}

	private void Update()
	{
	}
	public void Continue()
	{
        ShowVideoAd("2ooc1nkjb7a64bl294",
            (bol) => {
                if (bol)
                {

                    Time.timeScale = 1;
                    HideContinuePanel();


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
	}
	public void EndGame()
	{
		StartCoroutine(DisableTap());
		//Time.timeScale = 1;
		PlayerDied();
        //SceneManager.LoadScene("Main");
        ShowInterstitialAd("21ga1g2016chf3286m",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
    }
	public void GetScore(int getedScore)
	{
		this.score += getedScore;
		this.scoreText.text = this.score.ToString();
		this.scoreText.transform.DOScale(1.2f, 0.2f).OnComplete(delegate
		{
			this.scoreText.transform.DOScale(1f, 0.2f);
		});
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.scorePopupPrefab);
		gameObject.transform.position = new Vector3(gameObject.transform.position.x, Generator.instance.currentTube.startPositionY, gameObject.transform.position.z);
		gameObject.transform.GetChild(0).GetComponent<TextMesh>().text = "+" + getedScore.ToString();
	}
    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }
    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }
}
