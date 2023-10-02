using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomTextSelect : MonoBehaviour
{
    [SerializeField] List<string> textOptions = new List<string>();
    [SerializeField] TextMeshProUGUI textBox;

    private void Start()
    {
        textBox.text = textOptions[Random.Range(0, textOptions.Count)];
    }

    private void OnDisable()
    {
        textBox.text = textOptions[Random.Range(0, textOptions.Count)];
    }
}
