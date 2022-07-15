using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    private ViewModel viewModel;
    // Start is called before the first frame update
    void Start()
    {
        viewModel = ViewModel.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="Player")
        {
            viewModel.LoadNextLevel();
        }
    }
}
