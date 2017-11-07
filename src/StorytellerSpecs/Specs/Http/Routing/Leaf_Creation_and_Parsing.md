# Leaf Creation and Parsing

-> id = 66819200-37b6-426a-8523-ba59fa9578ff
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-11-07T13:22:52.5684390Z
-> tags = 

[Leaf]
|> CreateLeaf
    [table]
    |Route      |NodePath|HasSpread|Parameters   |
    |a/b/c      |a/b     |false    |NONE         |
    |a/b/:c     |a/b     |false    |c:2          |
    |a/b/{c}    |a/b     |false    |c:2          |
    |a/:b/c/:d  |a/*/c   |false    |b:1; d:3     |
    |a/{b}/c/{d}|a/*/c   |false    |b:1; d:3     |
    |...        |EMPTY   |True     |spread:0     |
    |a/...      |a       |True     |spread:1     |
    |a/:b/...   |a       |True     |b:1; spread:2|
    |a/b/...    |a/b     |True     |spread:2     |

~~~
