using System;
using System.Collections.Generic;

namespace Wellbeing.API.Services;

public class OutgoingMessage
{
    public Guid Id { get; set; }
    public string Target { get; set; }
    public Dictionary<string, string> Data { get; set; }
}