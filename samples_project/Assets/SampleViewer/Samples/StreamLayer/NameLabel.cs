using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameLabel : MonoBehaviour
{
	public string nameLabel;
	public Text text;
	public Slider slider;

	private void Start()
	{
		text.text = nameLabel;
		slider.value = 5;
	}
	// Update is called once per frame
	void Update()
	{
		transform.forward = Camera.main.transform.forward;
	}
}
