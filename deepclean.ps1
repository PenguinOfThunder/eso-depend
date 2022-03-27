Get-ChildItem -Recurse -Include bin,obj -Directory | % { Remove-Item -LiteralPath $_ -Force -Recurse }
