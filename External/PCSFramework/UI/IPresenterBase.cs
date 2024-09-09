using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPresenterBase
{
    UniTask InitializeAsync(int index=0);
}
