# Serialization Selection Rules

-> id = ad0d31a9-2f1d-4295-85fe-c49cddd0bd04
-> lifecycle = Acceptance
-> max-retries = 0
-> last-updated = 2017-03-09T08:45:38.6561310-06:00
-> tags = 

[SerializerSelection]
|> AvailableSerializers text/xml; text/json; text/yaml
|> Preference text/json; text/yaml
|> SerializationChoice
    [rows]
    |content   |channel               |envelope               |selection|
    |NULL      |EMPTY                 |EMPTY                  |text/json|
    |NULL      |text/xml, text/yaml   |EMPTY                  |text/xml |
    |NULL      |EMPTY                 |text/xml, text/yaml    |text/xml |
    |text/xml  |EMPTY                 |EMPTY                  |text/xml |
    |text/xml  |text/json, text/other |text/yaml              |text/xml |
    |text/other|EMPTY                 |EMPTY                  |NULL     |
    |NULL      |text/other, text/else |EMPTY                  |NULL     |
    |NULL      |text/other, text/json |EMPTY                  |text/json|
    |NULL      |EMPTY                 |text/other             |NULL     |
    |NULL      |EMPTY                 |text/other, text/json  |text/json|
    |NULL      |text/yaml             |text/xml               |text/xml |