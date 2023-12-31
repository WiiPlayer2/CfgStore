﻿namespace CfgStore.Application.Abstractions;

public interface ICfgFileStore<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, Seq<string>> List(string? path = default);

    Aff<RT, string> ReadText(string path);

    Aff<RT, Unit> WriteText(string path, string content);
}
