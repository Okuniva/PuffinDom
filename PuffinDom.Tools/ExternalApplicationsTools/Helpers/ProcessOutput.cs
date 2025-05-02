namespace PuffinDom.Tools.ExternalApplicationsTools.Helpers;

public class ProcessOutput
{
    public ProcessOutput(string data, bool isError = false)
    {
        Data = data;
        IsError = isError;
    }

    public string Data { get; }

    public bool IsError { get; }

    public override string ToString() => Data;
}