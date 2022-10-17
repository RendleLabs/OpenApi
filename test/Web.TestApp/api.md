# Contact Addresses API

> Version 1.0

## Path Table

| Method | Path | Description |
| --- | --- | --- |
| GET | [/api/{accountNumber}/addresses](#getapiaccountnumberaddresses) |  |
| POST | [/api/{accountNumber}/addresses](#postapiaccountnumberaddresses) |  |
| GET | [/api/{accountNumber}/addresses/{id}](#getapiaccountnumberaddressesid) |  |
| PUT | [/api/{accountNumber}/addresses/{id}](#putapiaccountnumberaddressesid) |  |

## Reference Table

| Name | Path | Description |
| --- | --- | --- |
| Address | [#/components/schemas/Address](#componentsschemasaddress) |  |
| IsoCountryCode | [#/components/schemas/IsoCountryCode](#componentsschemasisocountrycode) |  |

## Path Details

***

### [GET]/api/{accountNumber}/addresses

#### Responses

- 200 Addresses for customer

`application/json`

```ts
{
  // Unique identifier
  id?: string
  // The type of address
  type?: string
  line1?: string
  line2?: string
  line3?: string
  city?: string
  region?: string
  isoCountry?: string
  postalCode?: string
}[]
```

- 404 Customer not found

***

### [POST]/api/{accountNumber}/addresses

- Description  
Create customer address

#### RequestBody

- application/json

```ts
{
  // Unique identifier
  id?: string
  // The type of address
  type?: string
  line1?: string
  line2?: string
  line3?: string
  city?: string
  region?: string
  isoCountry?: string
  postalCode?: string
}
```

#### Responses

- 201 Address created successfully

- 400 Bad request

- 404 Customer account not found

***

### [GET]/api/{accountNumber}/addresses/{id}

#### Responses

- 200 Address for customer

`application/json`

```ts
{
  // Unique identifier
  id?: string
  // The type of address
  type?: string
  line1?: string
  line2?: string
  line3?: string
  city?: string
  region?: string
  isoCountry?: string
  postalCode?: string
}
```

- 404 Address not found

***

### [PUT]/api/{accountNumber}/addresses/{id}

- Description  
Update customer address

#### RequestBody

- application/json

```ts
{
  // Unique identifier
  id?: string
  // The type of address
  type?: string
  line1?: string
  line2?: string
  line3?: string
  city?: string
  region?: string
  isoCountry?: string
  postalCode?: string
}
```

#### Responses

- 200 Address updated successfully

- 400 Bad request

- 404 Customer account not found

## References

### #/components/schemas/Address

```ts
{
  // Unique identifier
  id?: string
  // The type of address
  type?: string
  line1?: string
  line2?: string
  line3?: string
  city?: string
  region?: string
  isoCountry?: string
  postalCode?: string
}
```

### #/components/schemas/IsoCountryCode

```ts
{
  "type": "string",
  "enum": [
    "AF",
    "AX",
    "AL",
    "DZ",
    "AS",
    "AD",
    "AO",
    "AI",
    "AQ",
    "AG",
    "AR",
    "AM",
    "AW",
    "AU",
    "AT",
    "AZ",
    "BS",
    "BH",
    "BD",
    "BB",
    "BY",
    "BE",
    "BZ",
    "BJ",
    "BM",
    "BT",
    "BO",
    "BQ",
    "BA",
    "BW",
    "BV",
    "BR",
    "IO",
    "BN",
    "BG",
    "BF",
    "BI",
    "CV",
    "KH",
    "CM",
    "CA",
    "KY",
    "CF",
    "TD",
    "CL",
    "CN",
    "CX",
    "CC",
    "CO",
    "KM",
    "CG",
    "CD",
    "CK",
    "CR",
    "CI",
    "HR",
    "CU",
    "CW",
    "CY",
    "CZ",
    "DK",
    "DJ",
    "DM",
    "DO",
    "EC",
    "EG",
    "SV",
    "GQ",
    "ER",
    "EE",
    "SZ",
    "ET",
    "FK",
    "FO",
    "FJ",
    "FI",
    "FR",
    "GF",
    "PF",
    "TF",
    "GA",
    "GM",
    "GE",
    "DE",
    "GH",
    "GI",
    "GR",
    "GL",
    "GD",
    "GP",
    "GU",
    "GT",
    "GG",
    "GN",
    "GW",
    "GY",
    "HT",
    "HM",
    "VA",
    "HN",
    "HK",
    "HU",
    "IS",
    "IN",
    "ID",
    "IR",
    "IQ",
    "IE",
    "IM",
    "IL",
    "IT",
    "JM",
    "JP",
    "JE",
    "JO",
    "KZ",
    "KE",
    "KI",
    "KP",
    "KR",
    "KW",
    "KG",
    "LA",
    "LV",
    "LB",
    "LS",
    "LR",
    "LY",
    "LI",
    "LT",
    "LU",
    "MO",
    "MG",
    "MW",
    "MY",
    "MV",
    "ML",
    "MT",
    "MH",
    "MQ",
    "MR",
    "MU",
    "YT",
    "MX",
    "FM",
    "MD",
    "MC",
    "MN",
    "ME",
    "MS",
    "MA",
    "MZ",
    "MM",
    "NA",
    "NR",
    "NP",
    "NL",
    "NC",
    "NZ",
    "NI",
    "NE",
    "NG",
    "NU",
    "NF",
    "MK",
    "MP",
    "NO",
    "OM",
    "PK",
    "PW",
    "PS",
    "PA",
    "PG",
    "PY",
    "PE",
    "PH",
    "PN",
    "PL",
    "PT",
    "PR",
    "QA",
    "RE",
    "RO",
    "RU",
    "RW",
    "BL",
    "SH",
    "KN",
    "LC",
    "MF",
    "PM",
    "VC",
    "WS",
    "SM",
    "ST",
    "SA",
    "SN",
    "RS",
    "SC",
    "SL",
    "SG",
    "SX",
    "SK",
    "SI",
    "SB",
    "SO",
    "ZA",
    "GS",
    "SS",
    "ES",
    "LK",
    "SD",
    "SR",
    "SJ",
    "SE",
    "CH",
    "SY",
    "TW",
    "TJ",
    "TZ",
    "TH",
    "TL",
    "TG",
    "TK",
    "TO",
    "TT",
    "TN",
    "TR",
    "TM",
    "TC",
    "TV",
    "UG",
    "UA",
    "AE",
    "GB",
    "US",
    "UM",
    "UY",
    "UZ",
    "VU",
    "VE",
    "VN",
    "VG",
    "VI",
    "WF",
    "EH",
    "YE",
    "ZM",
    "ZW"
  ]
}
```