# sample_http_client_proxy_get

通常設定では、プロキシの設定情報はHKCUに書かれる。（`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings`）

ProxySettingsPerUserを設定してプロキシをPC単位にしてある場合、HKCUの同じキーに書かれる。

どちらのケースでも、この実装でプロキシを取得することができた。PC単位にしてある場合に限り、Windowsサービス（LocalSystem）からもプロキシ情報が取得できる。

※この実装は.NET Core 3.1では動作しないことがすでに分かっている（WebProxyが実装に使われていないため）
