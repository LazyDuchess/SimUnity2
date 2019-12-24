using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ReferencingObject : MonoBehaviour
{
    public List<CachedAsset> references = new List<CachedAsset>();
    public void CleanUp()
    {
        foreach (var element in references)
        {
            element.Dereference();
        }
    }
    private void OnDestroy()
    {
        CleanUp();
    }
}
