using UnityEngine;
using System.Collections;

/// <summary>
/// We use this class to return the result of asynchronous operations
/// The advantage of this is that you can simply wait on it using yield in a coroutine
/// and the code will be linear unlike the callbacks approach
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T>
{
    private MonoBehaviour _owner;
    public bool isDone;
    public string error;

    public Result(MonoBehaviour owner)
    {
        this._owner = owner;
    }

    public bool HasError
    {
        get { return !string.IsNullOrEmpty(error); }
    }

    public T Value
    {
        get
        {
            if (isDone && !HasError)
                return value;
            throw new System.Exception("Result has errors but you want to get value");
        }
    }
    private T value;

    public void SetValue(T val)
    {
        value = val;
        isDone = true;
    }

    public void SetError(string err)
    {
        this.error = err;
        isDone = true;
    }

    public Coroutine WaitUntilDone()
    {
        return _owner.StartCoroutine(_WaitUntilDone());
    }

    private IEnumerator _WaitUntilDone()
    {
        while (!isDone)
            yield return null;
    }
}

