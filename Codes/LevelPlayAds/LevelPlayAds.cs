using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.unity3d.mediation;

// Init the SDK when implementing the Multiple Ad Units API for Interstitial and Banner formats, with Rewarded using legacy APIs 

public class LevelPlayAds : MonoBehaviour
{
    private LevelPlayBannerAd bannerAd;

    private void Start()
    {
    }

    private void OnEnable()
    {
    
// Init the SDK when implementing the Multiple Ad Units API for Interstitial and Banner formats, with Rewarded using legacy APIs 
        LevelPlayAdFormat[] legacyAdFormats = new[] { LevelPlayAdFormat.REWARDED };
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        IronSource.Agent.setMetaData("is_test_suite", "enable");
       
        LevelPlay.Init("yourId");
        IronSource.Agent.launchTestSuite();
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration obj)
    {
        LoadBanner();
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError obj)
    {
   
    }

    private void BannerOnAdExpandedEvent(LevelPlayAdInfo obj)
    {
       
    }

    private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo obj)
    {
     
    }

    private void BannerOnAdCollapsedEvent(LevelPlayAdInfo obj)
    {
      
    }

    private void BannerOnAdClickedEvent(LevelPlayAdInfo obj)
    {
       
    }

    private void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError obj)
    {
      
    }

    private void BannerOnAdDisplayedEvent(LevelPlayAdInfo obj)
    {
       
    }

    private void BannerOnAdLoadFailedEvent(LevelPlayAdError obj)
    {
       
    }

    private void BannerOnAdLoadedEvent(LevelPlayAdInfo obj)
    { 
        
    }

    private void OnDisable()
    {
        if(bannerAd == null)
        {
            return;
        }
        bannerAd.OnAdLoaded -= BannerOnAdLoadedEvent;
        bannerAd.OnAdLoadFailed -= BannerOnAdLoadFailedEvent;
        bannerAd.OnAdDisplayed -= BannerOnAdDisplayedEvent;
        bannerAd.OnAdDisplayFailed -= BannerOnAdDisplayFailedEvent;
        bannerAd.OnAdClicked -= BannerOnAdClickedEvent;
        bannerAd.OnAdCollapsed -= BannerOnAdCollapsedEvent;
        bannerAd.OnAdLeftApplication -= BannerOnAdLeftApplicationEvent;
        bannerAd.OnAdExpanded -= BannerOnAdExpandedEvent;
    }

    public void LoadBanner()
    {
      //  LevelPlayAdSize adSize = LevelPlayAdSize.CreateAdaptiveAdSize();
        bannerAd = new LevelPlayBannerAd("BannerID");
        bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
        bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
        bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
        bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
        bannerAd.OnAdClicked += BannerOnAdClickedEvent;
        bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
        bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
        bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;
        
        bannerAd.LoadAd();
        bannerAd.ShowAd();
    
    }

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
    //Banner CallBacks
}