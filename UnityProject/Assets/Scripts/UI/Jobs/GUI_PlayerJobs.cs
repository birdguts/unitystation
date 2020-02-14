﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GUI_PlayerJobs : MonoBehaviour
{
	public GameObject buttonPrefab;
	private CustomNetworkManager networkManager;
	public GameObject screen_Jobs;

	/// <summary>
	/// After the player selects a job this timer will be used to keep track of how long they've waited.
	/// When it is above 0 the timer will run and wait for the player to spawn.
	/// </summary>
	private float waitForSpawnTimer = 0;

	/// <summary>
	/// Number of seconds to wait after selecting a job. If the player does not spawn within that time the job selection re-opens.
	/// </summary>
	[SerializeField]
	[Range(0,15)]
	[Tooltip("Number of seconds to wait after selecting a job. If the player does not spawn within that time the job selection re-opens.")]
	private float waitForSpawnTimerMax = 6;

	/// <summary>
	/// Called when the player select a job selection button.
	/// Assigns the player that job and spawns them, unless the job was already taken.
	/// </summary>
	/// <param name="preference">The job associated with the button.</param>
	private void BtnOk(JobType preference)
	{
		if(waitForSpawnTimer > 0)
		{
			return; // Disallowing picking a job while another job has been selected.
		}
		SoundManager.Play("Click01");
		screen_Jobs.SetActive(false);
		PlayerManager.LocalViewerScript.CmdRequestJob(preference, PlayerManager.CurrentCharacterSettings);
		waitForSpawnTimer = waitForSpawnTimerMax;
	}

	void OnEnable()
	{
		screen_Jobs.SetActive(true);
	}

	/// <summary>
	/// If a role has been selected this waits for the player to spawn.
	/// </summary>
	private void Update()
	{
		if (waitForSpawnTimer > 0)
		{
			if (PlayerManager.HasSpawned)
			{
				// Job selection is finished, close the window.
				SoundManager.SongTracker.Stop();
				gameObject.SetActive(false);
			}

			waitForSpawnTimer -= Mathf.Max(0, Time.deltaTime);
			if (waitForSpawnTimer <= 0)
			{
				// Job selection failed, re-open it.
				SoundManager.Play("Click01");
				screen_Jobs.SetActive(true);
			}
		}
	}

	public void UpdateJobsList()
	{
		screen_Jobs.SetActive(false);

		foreach (Transform child in screen_Jobs.transform)
		{
			Destroy(child.gameObject);
		}

		foreach (Occupation occupation in OccupationList.Instance.Occupations)
		{
			JobType jobType = occupation.JobType;

			//NOTE: Commenting this out because it can actually be changed just by editing allowed occupation list,
			//doesn't need manual removal and this allows direct spawning as syndie for testing just by adding them
			//to that list
			// For nuke ops mode, syndis spawn via a different button
			// if (jobType == JobType.SYNDICATE)
			// {
			// 	continue;
			// }

			int active = GameManager.Instance.GetOccupationsCount(jobType);
			int available = GameManager.Instance.GetOccupationMaxCount(jobType);

			GameObject occupationGO = Instantiate(buttonPrefab, screen_Jobs.transform);

			// This line was added for unit testing - but now it's only rewrite occupations meta
			//occupation.name = jobType.ToString();

			var color = occupation.ChoiceColor;

			occupationGO.GetComponent<Image>().color = color;
			occupationGO.GetComponentInChildren<Text>().text = occupation.DisplayName + " (" + active + " of " + available + ")";
			occupationGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

			// Disabled button for full jobs
			if (active >= available)
			{
				occupationGO.GetComponentInChildren<Button>().interactable = false;
			}
			else // Enabled button with listener for vacant jobs
			{
				occupationGO.GetComponent<Button>().onClick.AddListener(() => { BtnOk(jobType); });
			}

			occupationGO.SetActive(true);
		}
		screen_Jobs.SetActive(true);
	}
}