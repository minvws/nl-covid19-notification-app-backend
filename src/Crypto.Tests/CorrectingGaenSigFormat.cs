// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests
{
    public class CorrectingGaenSigFormat
    {
//
// Test generated with:
//
// set -e
// repeat 30 {
//    # Generate pub/keypare
//    openssl ecparam -name secp256k1 -genkey -noout -out ec.key
//    openssl ec -in ec.key -pubout -out ec.pub
//
//    # Generate payload to sign
//    openssl rand 1300 > payload
//
//    # Sign payload & check
//    #
//    openssl dgst -sha256 -sign ec.key payload > signature.bin
//    openssl dgst -sha256 -verify ec.pub -signature signature.bin payload
//
//    # Extract the two 32 byte integers; and output these concatenated
//    # much like .NET its cert.GetECDsaPrivateKey().SignData() does.
//
//    openssl asn1parse -inform DER -in signature.bin |\
//        grep INTEGER |\
//        sed -e 's/.*://' |\
//        xxd -r -p - | base64 > signature.r64
//
//    echo \[DataRow\(\"`base64 signature.r64`\",\"`base64 signature.bin`\"\)\]
// }
        [Theory]
        [InlineData("NtnqKDHatsWQYS1TKjxD+73qQ5BWz2zAEWqzpFs8BCyQ+LtoaMxfxtsM0o/HVUMsce2YTNSQM39Dmf6+1h8UJA==", "MEUCIDbZ6igx2rbFkGEtUyo8Q/u96kOQVs9swBFqs6RbPAQsAiEAkPi7aGjMX8bbDNKPx1VDLHHtmEzUkDN/Q5n+vtYfFCQ=")]
        [InlineData("9CqMHwPaGnEOOW28vuaw9btDgwe7chpWmMMUJJFSGykTGJT9yk86fDPuFivi2S27FdOYiAwSIxvx1ZxLaly+Tw==", "MEUCIQD0KowfA9oacQ45bby+5rD1u0ODB7tyGlaYwxQkkVIbKQIgExiU/cpPOnwz7hYr4tktuxXTmIgMEiMb8dWcS2pcvk8=")]
        [InlineData("h0oJTLbvyE7jkfEhnqn5YIMI0g+K67s0OAqpj8Gb2tnIQEmw8qZ/M4tc0fcUIX0gjxyUqwTFO5nRUi8FN+UZaw==", "MEYCIQCHSglMtu/ITuOR8SGeqflggwjSD4rruzQ4CqmPwZva2QIhAMhASbDypn8zi1zR9xQhfSCPHJSrBMU7mdFSLwU35Rlr")]
        [InlineData("cuUOKUuBjdJQvGt0TaxJJSnd/ByYcgeim2ndEpzYfwakW6ZWDg+sD5Bxuw0va0Uhrjuwnx4toAS0kShQFHSHVA==", "MEUCIHLlDilLgY3SULxrdE2sSSUp3fwcmHIHoptp3RKc2H8GAiEApFumVg4PrA+QcbsNL2tFIa47sJ8eLaAEtJEoUBR0h1Q=")]
        [InlineData("YVrPWa3Qp+yKsorY+FNJz4bslSqRp80oO6B3lDKfFvupvPvaRNz3EkJAxRsfvLwb3W4D/yZvQbGzsK1m+pMjVg==", "MEUCIGFaz1mt0KfsirKK2PhTSc+G7JUqkafNKDugd5Qynxb7AiEAqbz72kTc9xJCQMUbH7y8G91uA/8mb0Gxs7CtZvqTI1Y=")]
        [InlineData("33tCqMxPZFj4rrNn+IvSqcg51jrhr76X9RzldzWbUJCYMcRQWPFQ9tylEbBhVTRuhhGniHMH5mYCSD4JzE8W7Q==", "MEYCIQDfe0KozE9kWPius2f4i9KpyDnWOuGvvpf1HOV3NZtQkAIhAJgxxFBY8VD23KURsGFVNG6GEaeIcwfmZgJIPgnMTxbt")]
        [InlineData("YYal1Qy7qeTWBuNBfCqEteuhaermnO8CyPNMA0adlBpY6kQmSVvLrkpi2Brp/x+5sAawmrkhfCVR0cs+mkDJoQ==", "MEQCIGGGpdUMu6nk1gbjQXwqhLXroWnq5pzvAsjzTANGnZQaAiBY6kQmSVvLrkpi2Brp/x+5sAawmrkhfCVR0cs+mkDJoQ==")]
        [InlineData("ezYW3tIkilAWZAKjgN1EbRFf8pQybIbXxNR4EmlQhN/74e81RFIxlgJ1lsB+I2HsVxqAMoRUfRQZCj5h0jCkzw==", "MEUCIHs2Ft7SJIpQFmQCo4DdRG0RX/KUMmyG18TUeBJpUITfAiEA++HvNURSMZYCdZbAfiNh7FcagDKEVH0UGQo+YdIwpM8=")]
        [InlineData("b0BUk787/qreW3FLJ+vpoYNAhQSnRvr1nPwM0KEnYeKiKTj5mMc0ceRBIs0F2v6p5Ku0vrZPWSp3hZE90exwpA==", "MEUCIG9AVJO/O/6q3ltxSyfr6aGDQIUEp0b69Zz8DNChJ2HiAiEAoik4+ZjHNHHkQSLNBdr+qeSrtL62T1kqd4WRPdHscKQ=")]
        [InlineData("4gWLhDGRYowlWihW8rKNuQBv59RdojGaoxUVjBkWzTb+lwCpVAQiFqlWANwo+KD1N8L+24qhcJVvezO7YmL0NQ==", "MEYCIQDiBYuEMZFijCVaKFbyso25AG/n1F2iMZqjFRWMGRbNNgIhAP6XAKlUBCIWqVYA3Cj4oPU3wv7biqFwlW97M7tiYvQ1")]
        [InlineData("b/PqJCuvjQMGgVTjV28c3HPg1w/wtdC3lf7eedl2DAkjX787S9xSq9pVuxtTaiAlP+w5mCbFHC6Y6ely422kdQ==", "MEQCIG/z6iQrr40DBoFU41dvHNxz4NcP8LXQt5X+3nnZdgwJAiAjX787S9xSq9pVuxtTaiAlP+w5mCbFHC6Y6ely422kdQ==")]
        [InlineData("i4y0DeLxbn4pj9V6AJnWizuLcWUVvslcwcyB6OK5dhzKpmgNkyQxxZygOTA5xKqQExYGjvjwCJfABa3dm98ncw==", "MEYCIQCLjLQN4vFufimP1XoAmdaLO4txZRW+yVzBzIHo4rl2HAIhAMqmaA2TJDHFnKA5MDnEqpATFgaO+PAIl8AFrd2b3ydz")]
        [InlineData("XobW1IkxvURGdXKjKhEJxyWfnaPAU8SrLA2oehJpSCt8ghUqT9q4jvne4PvdeiJItFjDybtSSEfeKjJM8+dbAw==", "MEQCIF6G1tSJMb1ERnVyoyoRCccln52jwFPEqywNqHoSaUgrAiB8ghUqT9q4jvne4PvdeiJItFjDybtSSEfeKjJM8+dbAw==")]
        [InlineData("eWKlGrg7O+TdqdZThoMFdlCftZQebGTDUrRQyLCLw4wEBFkwHL/d9GA/F/qKAEvN4UdcSa4xmYgbeofzs9DIZw==", "MEQCIHlipRq4Ozvk3anWU4aDBXZQn7WUHmxkw1K0UMiwi8OMAiAEBFkwHL/d9GA/F/qKAEvN4UdcSa4xmYgbeofzs9DIZw==")]
        [InlineData("e1Dv/Av9OPoDpyzPjMjJDouYQTTsI+TokhLEsk58S0BadVOJmkk6vFJro90aIQEHDdPy470JC1RQapoG9KRgSw==", "MEQCIHtQ7/wL/Tj6A6csz4zIyQ6LmEE07CPk6JISxLJOfEtAAiBadVOJmkk6vFJro90aIQEHDdPy470JC1RQapoG9KRgSw==")]
        [InlineData("eOKYU5eS1h5OfJVQZhsgs6Bn3UxONYQMMhw4eyNlfib+KfNpCXM6URVlAIts8z7bquraJ5BEFIQb9oAPUAlgsA==", "MEUCIHjimFOXktYeTnyVUGYbILOgZ91MTjWEDDIcOHsjZX4mAiEA/inzaQlzOlEVZQCLbPM+26rq2ieQRBSEG/aAD1AJYLA=")]
        [InlineData("Bi760/CUNXjjC0ksv9wOWnnUVvKxCtIXuZYniKLTscTZGLieBT3vqaFBfMvNYPZ/2ex+8ASNACqascQQ6lO3iQ==", "MEUCIAYu+tPwlDV44wtJLL/cDlp51FbysQrSF7mWJ4ii07HEAiEA2Ri4ngU976mhQXzLzWD2f9nsfvAEjQAqmrHEEOpTt4k=")]
        [InlineData("wZ0yb4fBTEP+b9kVkIm5fwyElVx+xBDaARhLwS+eHHnVliM0ys+b8DF8wRlaCYe1cb0Cnr8Hu79z2P6+1k14aA==", "MEYCIQDBnTJvh8FMQ/5v2RWQibl/DISVXH7EENoBGEvBL54ceQIhANWWIzTKz5vwMXzBGVoJh7VxvQKevwe7v3PY/r7WTXho")]
        [InlineData("ip6PHv9cSJAtmPqhS4C11S+pQg28gAf+qLV86uqoMmBFI9K2fg1oB1yHckk9BH7GcVVZ6eK2wUNjpuTN3+gDIA==", "MEUCIQCKno8e/1xIkC2Y+qFLgLXVL6lCDbyAB/6otXzq6qgyYAIgRSPStn4NaAdch3JJPQR+xnFVWenitsFDY6bkzd/oAyA=")]
        [InlineData("CGZaEdJYR3qQfzNPFtTzMsTSWguKZKUMIL21exB/Pj982wVUUjeFIPF0N18o79XyxTALX63MG/sqXlszdbK6iw==", "MEQCIAhmWhHSWEd6kH8zTxbU8zLE0loLimSlDCC9tXsQfz4/AiB82wVUUjeFIPF0N18o79XyxTALX63MG/sqXlszdbK6iw==")]
        [InlineData("xE796HUvyKf7ektGOIe9DHjMxv7zId+W2mL/L6FUiw2qRvoaPd8lqVVxRZo0Wbcjd5qgYWBOJ90EezkyXdzbww==", "MEYCIQDETv3odS/Ip/t6S0Y4h70MeMzG/vMh35baYv8voVSLDQIhAKpG+ho93yWpVXFFmjRZtyN3mqBhYE4n3QR7OTJd3NvD")]
        [InlineData("ppxzFwi+inTPZkRbjcvGvtIs2AYlXNW2d/4oEe5QqlYNcczx9yvOjjrrksyVHfM7NhrM2KsXFKrJ5Fecjbix7w==", "MEUCIQCmnHMXCL6KdM9mRFuNy8a+0izYBiVc1bZ3/igR7lCqVgIgDXHM8fcrzo4665LMlR3zOzYazNirFxSqyeRXnI24se8=")]
        [InlineData("XBQGWbYCVW+y/LXUohBifKgl1Wf5nP8p+7CsdBwP7+1IvxwnK2OsB61cBJtTXSx7HqjsnZRYfqRNWzOQ33S4AQ==", "MEQCIFwUBlm2AlVvsvy11KIQYnyoJdVn+Zz/KfuwrHQcD+/tAiBIvxwnK2OsB61cBJtTXSx7HqjsnZRYfqRNWzOQ33S4AQ==")]
        [InlineData("TuiwSQIk5142JPKO/vmY9J95i/rpHCp/5QEINykrwHAvKtg4CCSiAlST8RPA9V33Il+8IxYjaE7jIjUK8b5++A==", "MEQCIE7osEkCJOdeNiTyjv75mPSfeYv66Rwqf+UBCDcpK8BwAiAvKtg4CCSiAlST8RPA9V33Il+8IxYjaE7jIjUK8b5++A==")]
        [InlineData("sTPdPYsSONNeuav0xUX4zjBjICcB8VcOjRXOma6vxMBYu0jNgqYno/WGIYVNYCXL6oXv7rrzoc4tWieiEXcfzQ==", "MEUCIQCxM909ixI40165q/TFRfjOMGMgJwHxVw6NFc6Zrq/EwAIgWLtIzYKmJ6P1hiGFTWAly+qF7+6686HOLVonohF3H80=")]
        [InlineData("nOz+m2c/YhugAcB9GRQ7rOrx5987MkGjs5MRyMKZv5hP/3FhiNChxOqngC9yI/rcuEq8IOZ2zTh6h6kuEPOYxg==", "MEUCIQCc7P6bZz9iG6ABwH0ZFDus6vHn3zsyQaOzkxHIwpm/mAIgT/9xYYjQocTqp4AvciP63LhKvCDmds04eoepLhDzmMY=")]
        [InlineData("Xmq7xhGL55hBhxkZyscMH9DYLFObiXApZNyLXD7+5wd865k9oJ9c8TJ2MvzrYjt5kAa9DwBsv0006Wo1Xalsfg==", "MEQCIF5qu8YRi+eYQYcZGcrHDB/Q2CxTm4lwKWTci1w+/ucHAiB865k9oJ9c8TJ2MvzrYjt5kAa9DwBsv0006Wo1Xalsfg==")]
        [InlineData("Unekf6vyR2kGhk9tSvCtHIVjsNZ/PksSr6TiMljcIijSj4murXZbMTqrKwYCZvaP3L0eOv86Dqgz9lIoNGnFkg==", "MEUCIFJ3pH+r8kdpBoZPbUrwrRyFY7DWfz5LEq+k4jJY3CIoAiEA0o+Jrq12WzE6qysGAmb2j9y9Hjr/Og6oM/ZSKDRpxZI=")]
        [InlineData("idQoEkc+fvqZ9RPr7VAaZBvDzIiqjBQ6iuDbaYwXfQmV4yS4ZTWZ5jEQ83lSBEqzyi0vlAj+PIo65hDo/bFC6w==", "MEYCIQCJ1CgSRz5++pn1E+vtUBpkG8PMiKqMFDqK4NtpjBd9CQIhAJXjJLhlNZnmMRDzeVIESrPKLS+UCP48ijrmEOj9sULr")]
        [InlineData("S/82nozqc0CDxw4dVU8hv82DQts/Vk3weXgIxRLqmapf6ayOAROnR7jtzAWaZEkuIH+HlYuOUInOZgNBd9ckfQ==", "MEQCIEv/Np6M6nNAg8cOHVVPIb/Ng0LbP1ZN8Hl4CMUS6pmqAiBf6ayOAROnR7jtzAWaZEkuIH+HlYuOUInOZgNBd9ckfQ==")]
        [InlineData("itbYr0Fn+Bch+uZnw4MZO4WQa4/B7iL2+9Jo+Uby4+KGyt68R6mMfzYyRaFnFxP84+goq2wYaMwsYCNK31r8IQ==", "MEYCIQCK1tivQWf4FyH65mfDgxk7hZBrj8HuIvb70mj5RvLj4gIhAIbK3rxHqYx/NjJFoWcXE/zj6CirbBhozCxgI0rfWvwh")]
        [InlineData("2w06NNLSoQSK2wxSAj0bfZSbneRHVTBPwf5eJhsPwaxFt30dw5wxlw1kFHFaYHGX6MQdj1udK4AKRwr6o7410Q==", "MEUCIQDbDTo00tKhBIrbDFICPRt9lJud5EdVME/B/l4mGw/BrAIgRbd9HcOcMZcNZBRxWmBxl+jEHY9bnSuACkcK+qO+NdE=")]
        [InlineData("1s6Fde6FvTJ3IfrbBmXy6hyoDhWrhZz09Gs6u4d0O6K3xd1YpRKQG9o2NJVujf0lGcY+G19cqRI5DKABNwDCww==", "MEYCIQDWzoV17oW9Mnch+tsGZfLqHKgOFauFnPT0azq7h3Q7ogIhALfF3VilEpAb2jY0lW6N/SUZxj4bX1ypEjkMoAE3AMLD")]
        [InlineData("OhrcwQWwmv0BAGEYsfA5SeR5khJCpBkiQzkGQi5owt+LsKz+edVVC2RtNYFcT4GEa9i84ucjSfWL64q23qCH7g==", "MEUCIDoa3MEFsJr9AQBhGLHwOUnkeZISQqQZIkM5BkIuaMLfAiEAi7Cs/nnVVQtkbTWBXE+BhGvYvOLnI0n1i+uKtt6gh+4=")]
        [InlineData("9IRmu7rxT5L6iivjvEkk54LnEHnV9/F4HgbT8JqdYuk7yzM4ett3XSpBA9uTlEitt1dxxgc2eHJdWQcSrzCJDw==", "MEUCIQD0hGa7uvFPkvqKK+O8SSTngucQedX38XgeBtPwmp1i6QIgO8szOHrbd10qQQPbk5RIrbdXccYHNnhyXVkHEq8wiQ8=")]
        [InlineData("wlDwRmZAuUcKjfOOCXv2GBCJDs9cKt0ZOAaQCA3MNQdZ+23JMYTlc6J5oQKW7bKNxgbkyb7rVWNMXYB2P93c+g==", "MEUCIQDCUPBGZkC5RwqN844Je/YYEIkOz1wq3Rk4BpAIDcw1BwIgWfttyTGE5XOieaEClu2yjcYG5Mm+61VjTF2Adj/d3Po=")]
        [InlineData("n74WypaIKrdKPeZ7ipKFq2Udyn29KkR45N3BYlyz9PtctzhUFlHqtGiCa9Z/coKBBP0oAZiB3NmVcqEdXOUT1A==", "MEUCIQCfvhbKlogqt0o95nuKkoWrZR3Kfb0qRHjk3cFiXLP0+wIgXLc4VBZR6rRogmvWf3KCgQT9KAGYgdzZlXKhHVzlE9Q=")]
        [InlineData("T3LMgffOoeblbfuft4RzVprzPqoBfNBtMf22cHB7wFYaoYMcqKImXoPjKlh121Rlx0QDu+1Zi4S2GrtN0TZ4MQ==", "MEQCIE9yzIH3zqHm5W37n7eEc1aa8z6qAXzQbTH9tnBwe8BWAiAaoYMcqKImXoPjKlh121Rlx0QDu+1Zi4S2GrtN0TZ4MQ==")]
        [InlineData("/clA3rP4itLePsdFkWB+J+1K5DiI5kskLkvJm3pyVWS5WclbFFzdACLcfpztKEmHxo2d50zz3G8kJwR5V9lcjw==", "MEYCIQD9yUDes/iK0t4+x0WRYH4n7UrkOIjmSyQuS8mbenJVZAIhALlZyVsUXN0AItx+nO0oSYfGjZ3nTPPcbyQnBHlX2VyP")]
        [InlineData("Nk8htJPR4fUikD+M/HX6tirDjwYChoHbEEb64a3TdqvxXsgdE2fEzhkR+2+mmH5J6gu1HkmUI/FHrpG01/evQw==", "MEUCIDZPIbST0eH1IpA/jPx1+rYqw48GAoaB2xBG+uGt03arAiEA8V7IHRNnxM4ZEftvpph+SeoLtR5JlCPxR66RtNf3r0M=")]
        [InlineData("pIymao0C9p807BKL1d5PLlnnirXFSydp01YLxLoUL7Guyr7TsDnU2hAc3N8QZ9OsxDUHgRaV6H8b93LLA+F+zw==", "MEYCIQCkjKZqjQL2nzTsEovV3k8uWeeKtcVLJ2nTVgvEuhQvsQIhAK7KvtOwOdTaEBzc3xBn06zENQeBFpXofxv3cssD4X7P")]
        [InlineData("BzkDcOJC+DYSToVug3cQXT3WW8UdnivJiVgnQJyjbzhyaUC+az4xAmrFk/QGoDzuMkUYCf6NxtYWIpz6u795lA==", "MEQCIAc5A3DiQvg2Ek6FboN3EF091lvFHZ4ryYlYJ0Cco284AiByaUC+az4xAmrFk/QGoDzuMkUYCf6NxtYWIpz6u795lA==")]
        [InlineData("eta/GsI9cCcnZyknua8FW57XVYlGQrTsg0i+9FBqsDUf5xtCph763oawNuoyKG2d+Wqa2DlDLroEykfD3ZupJw==", "MEQCIHrWvxrCPXAnJ2cpJ7mvBVue11WJRkK07INIvvRQarA1AiAf5xtCph763oawNuoyKG2d+Wqa2DlDLroEykfD3ZupJw==")]
        [InlineData("lyD3Ieh3q903Wh2CqhELTHTxn9CCsxF8nq6o1zSaLlx/xl7UTCbEuNAjdHL2Rr3iSgBs9HP/F5QXcdJeVuCf5A==", "MEUCIQCXIPch6Her3TdaHYKqEQtMdPGf0IKzEXyerqjXNJouXAIgf8Ze1EwmxLjQI3Ry9ka94koAbPRz/xeUF3HSXlbgn+Q=")]
        [InlineData("aRCJ7UAwqjR5ZZGqRCw9Rk50EG1Skp9VJ2Rb/WuroP9Qt4QCAVFH4eLMuNo0NxMH25N0Hy2pixMGAReyEYFGvg==", "MEQCIGkQie1AMKo0eWWRqkQsPUZOdBBtUpKfVSdkW/1rq6D/AiBQt4QCAVFH4eLMuNo0NxMH25N0Hy2pixMGAReyEYFGvg==")]
        [InlineData("vEyhXS3U+h44WfJHtCgFmyM+fm3sXNeB0Bwyxs7LRTkC2XYBNWp85USa4JzRzLprpekEk13Fp7ZH5DTblFXHgw==", "MEUCIQC8TKFdLdT6HjhZ8ke0KAWbIz5+bexc14HQHDLGzstFOQIgAtl2ATVqfOVEmuCc0cy6a6XpBJNdxae2R+Q025RVx4M=")]



        public void Batch1(string input, string expected)
        {
            var inputBytes = Convert.FromBase64String(input);
            var expectedBytes = Convert.FromBase64String(expected);

            Trace.WriteLine($"Input:            {ToHex(inputBytes)}");
            Trace.WriteLine($"Expected: {ToHex(expectedBytes)}");
            Trace.WriteLine($"Output:   {ToHex(new X962PackagingFix().Format(inputBytes))}");

            Assert.Equal(expectedBytes, new X962PackagingFix().Format(inputBytes));
        }

        [Theory]
        [InlineData("AAwEowsFZjAOp1Mlj7vSf0QOCIJJ1+gah6dRZai8qMkYZnL6ja+9uzJjjB3V/Kypb0UtsyYkIaEvtI2l0if+yA==", "MEMCHwwEowsFZjAOp1Mlj7vSf0QOCIJJ1+gah6dRZai8qMkCIBhmcvqNr727MmOMHdX8rKlvRS2zJiQhoS+0jaXSJ/7I")]
        [InlineData("Yxfsi5CkjIXPeocXan8nZNaqmo/QJASPsLFt0PVL2dEABvrEuj5cxjQAsMVUjqQ1wX8PvovaoGraEftdfgIsCw==", "MEMCIGMX7IuQpIyFz3qHF2p/J2TWqpqP0CQEj7CxbdD1S9nRAh8G+sS6PlzGNACwxVSOpDXBfw++i9qgatoR+11+AiwL")]
        [InlineData("AFWnj6sgI5gSuza3H7UVXpuEAiKLZfQ7ljSr9brnqmh5OQGaBgQEAh7RjBxMJjTVhAZ1WrnaSHJjCLj8jC+4bA==", "MEMCH1Wnj6sgI5gSuza3H7UVXpuEAiKLZfQ7ljSr9brnqmgCIHk5AZoGBAQCHtGMHEwmNNWEBnVaudpIcmMIuPyML7hs")]
        [InlineData("WWmHXVSsrtu98ghDAEFtOzH0kkbfRRRiu/Y1zLHdvpwAbO+dTPGGIQ7U3bKVuEbwyNkfwNy9m5YIXNgwKytghw==", "MEMCIFlph11UrK7bvfIIQwBBbTsx9JJG30UUYrv2Ncyx3b6cAh9s751M8YYhDtTdspW4RvDI2R/A3L2blghc2DArK2CH")]
        [InlineData("L9cG0TzXiRlIg0JmeKxfamjfT3pqghzqAneMk2z/XqUAfQkqQ4gfUDPdh8F1PjlXs7T63MIkoaD9RUyfXrAXIw==", "MEMCIC/XBtE814kZSINCZnisX2po3096aoIc6gJ3jJNs/16lAh99CSpDiB9QM92HwXU+OVeztPrcwiShoP1FTJ9esBcj")]
        [InlineData("f+vUPe/ySqrMBdCpQ8/E0BIR+imCxpD9hNDZVNzQ4B8AD7gkaI4r/VSBiFPXqepeHeKummubBlbr3E4jmEPOsg==", "MEMCIH/r1D3v8kqqzAXQqUPPxNASEfopgsaQ/YTQ2VTc0OAfAh8PuCRojiv9VIGIU9ep6l4d4q6aa5sGVuvcTiOYQ86y")]
        [InlineData("JlbTfEq8q3WXvD5nCwqt35y8xZSz0wpclJOQvzb6d9AALT2RcXjwoY/YTKiwh/GzUIZxP06Tr/27v8uo4uyfOQ==", "MEMCICZW03xKvKt1l7w+ZwsKrd+cvMWUs9MKXJSTkL82+nfQAh8tPZFxePChj9hMqLCH8bNQhnE/TpOv/bu/y6ji7J85")]
        [InlineData("x5E945VtzyJmNuWXe9QHd0bPtMmt+bgK51kzRhcswZwAABzcwzWHOEub0MY+/VzCnRuhVMuXAhrZ/p2Y7bSqEA==", "MEMCIQDHkT3jlW3PImY25Zd71Ad3Rs+0ya35uArnWTNGFyzBnAIeHNzDNYc4S5vQxj79XMKdG6FUy5cCGtn+nZjttKoQ")]
        [InlineData("flyonJEx1hQqg3yOXVfYRYDzaN0jQtf44ZMiGu5zTm4AVhJ7xvixgtFHJNUXP8GGGQb/gWAhI+I8hLSaOAclBA==", "MEMCIH5cqJyRMdYUKoN8jl1X2EWA82jdI0LX+OGTIhruc05uAh9WEnvG+LGC0Uck1Rc/wYYZBv+BYCEj4jyEtJo4ByUE")]
        [InlineData("AFUSAwZyDpH7s9CcoYIXXjslOGNA1vq252rhY5pt5kEQ1YKOvWL78ARbj/P7kz4YqIqL1rt3WTjhjz5stz5qTg==", "MEMCH1USAwZyDpH7s9CcoYIXXjslOGNA1vq252rhY5pt5kECIBDVgo69YvvwBFuP8/uTPhioiovWu3dZOOGPPmy3PmpO")]
        [InlineData("CWvAVpQSINa4nxk2i2yTmTc0lzJv9GCMpPaKax9AoIIAJ2qJ8Q09XwWPZRxsIsvbFZW/Pid9wuQjfV25UvxHoQ==", "MEMCIAlrwFaUEiDWuJ8ZNotsk5k3NJcyb/RgjKT2imsfQKCCAh8naonxDT1fBY9lHGwiy9sVlb8+J33C5CN9XblS/Eeh")]
        [InlineData("G4Qua4wLfUJ51aJYuhE4GCP1j8eH5BJAgQ0itN9+o6UARhqayODu+eDxUk1Im5HS5MczPLRxqC0+SQLhd1vaSg==", "MEMCIBuELmuMC31CedWiWLoROBgj9Y/Hh+QSQIENIrTffqOlAh9GGprI4O754PFSTUibkdLkxzM8tHGoLT5JAuF3W9pK")]
        [InlineData("WAFIjPppk5X1KBiRDFVwPdKXm1s7naVjqw6CSTAHZLMAcFaD6lrjHG97jp7/LRIgITUwgRl4o8jD6vMeqj3DvA==", "MEMCIFgBSIz6aZOV9SgYkQxVcD3Sl5tbO52lY6sOgkkwB2SzAh9wVoPqWuMcb3uOnv8tEiAhNTCBGXijyMPq8x6qPcO8")]
        [InlineData("bfHr+r989DkrYTgoobg+knVLhh2JzKLoxCRwPYIXNl4AOno6eNt+uXDToYKS40AqC3oaxaj9Z3b2fuDXyvREeA==", "MEMCIG3x6/q/fPQ5K2E4KKG4PpJ1S4Ydicyi6MQkcD2CFzZeAh86ejp42365cNOhgpLjQCoLehrFqP1ndvZ+4NfK9ER4")]
        [InlineData("AHhJ705vD/jmSaIto8eNPJhc0E3gMaX0mWMDxV+gRw9x2Ctmt3AnhWIWsYIkP3WLp5WTisTghP3nRu84JeB0ZQ==", "MEMCH3hJ705vD/jmSaIto8eNPJhc0E3gMaX0mWMDxV+gRw8CIHHYK2a3cCeFYhaxgiQ/dYunlZOKxOCE/edG7zgl4HRl")]
        [InlineData("VhkmOMVanpkt5p7SQZk91fOd+1Q6O/yj/2wKDuY9SBQAHM1WTAABBdGouN3ieRdDpOzuFW0nDujd5RKxrWR9FA==", "MEMCIFYZJjjFWp6ZLeae0kGZPdXznftUOjv8o/9sCg7mPUgUAh8czVZMAAEF0ai43eJ5F0Ok7O4VbScO6N3lErGtZH0U")]
        [InlineData("AATM/6+08f0CR5PPNkrTmr5Vnjk6sQTm5HV5CGZB/79z/Of/24QZPkM+MzaBELoSezV3F/y7grWE3pRD50H/tA==", "MEMCHwTM/6+08f0CR5PPNkrTmr5Vnjk6sQTm5HV5CGZB/78CIHP85//bhBk+Qz4zNoEQuhJ7NXcX/LuCtYTelEPnQf+0")]
        [InlineData("AFsMHqbdABHKVdAMh8OmZ8V5+V5RXk4ziBnJJXgIp+5WI4YtfNP3EW0jrFaIw2frGD4c4lJ+oyqbO3eo6haRoQ==", "MEMCH1sMHqbdABHKVdAMh8OmZ8V5+V5RXk4ziBnJJXgIp+4CIFYjhi180/cRbSOsVojDZ+sYPhziUn6jKps7d6jqFpGh")]
        [InlineData("XqYOkH1vYxFqhf3r9DhNpVhpbyW8wO5E7+z+6cHuQjAAe75Per89UcThi6C8WffZujqYvdUUxEbjnYv63NZ7og==", "MEMCIF6mDpB9b2MRaoX96/Q4TaVYaW8lvMDuRO/s/unB7kIwAh97vk96vz1RxOGLoLxZ99m6Opi91RTERuOdi/rc1nui")]
        [InlineData("cCfaiV/e27sVQyc2xOAK2/okO+oH3RKsDxh7gerxGbUAYigNKQu0QA4CPyExDTZU2BhOidXnzHbAfVgfkIN0KQ==", "MEMCIHAn2olf3tu7FUMnNsTgCtv6JDvqB90SrA8Ye4Hq8Rm1Ah9iKA0pC7RADgI/ITENNlTYGE6J1efMdsB9WB+Qg3Qp")]
        [InlineData("CU2e5gUtS9KzfW+eJzt3AJsRAOK1Ps+DuSRVHc9E/hgAXLyPABzkuSbjIuatIuYdvJmqbPpmbuAV1ItHVLUMuw==", "MEMCIAlNnuYFLUvSs31vnic7dwCbEQDitT7Pg7kkVR3PRP4YAh9cvI8AHOS5JuMi5q0i5h28maps+mZu4BXUi0dUtQy7")]
        [InlineData("x8NQjFdcXkjWbF9OfVD8IoerL/sJyTRBEV7MWhESaRQAADN7Dip5HF+s3yUP3lZTimrqst2ZsgJFk5wl4OpTyg==", "MEMCIQDHw1CMV1xeSNZsX059UPwih6sv+wnJNEERXsxaERJpFAIeM3sOKnkcX6zfJQ/eVlOKauqy3ZmyAkWTnCXg6lPK")]
        [InlineData("Y48oZj/JGEpfFEX8tPUu1TtIMn5U76tbl2OX9eySrTIAeXWcjBdIdFoPQ04dTUSxzW0ivDbmmTgRWugwHDl23g==", "MEMCIGOPKGY/yRhKXxRF/LT1LtU7SDJ+VO+rW5djl/Xskq0yAh95dZyMF0h0Wg9DTh1NRLHNbSK8NuaZOBFa6DAcOXbe")]
        [InlineData("U9OmCSIibIuTXeweS9EVVlD/TLk/mW5pioAOmX5/38oAI1jmMU/ryAQoVNa13/nCNajc/wP2lr5k1eFuzrwgQg==", "MEMCIFPTpgkiImyLk13sHkvRFVZQ/0y5P5luaYqADpl+f9/KAh8jWOYxT+vIBChU1rXf+cI1qNz/A/aWvmTV4W7OvCBC")]
        public void Batch2(string input, string expected)
        {
            var inputBytes = Convert.FromBase64String(input);
            var expectedBytes = Convert.FromBase64String(expected);

            Trace.WriteLine($"Input:          {ToHex(inputBytes)}");
            Trace.WriteLine($"Expected: {ToHex(expectedBytes)}");
            Trace.WriteLine($"Output:   {ToHex(new X962PackagingFix().Format(inputBytes))}");

            Assert.Equal(expectedBytes, new X962PackagingFix().Format(inputBytes));
        }


        // Simple case - full length.
        // P
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Q
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  68 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //   36:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("ESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MEQCIBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIAiARIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==")]
        // Lots of prefixes
        // P
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 FF 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 FF 00 00 00 00 00 00 00 00 00
        // Expected output:
        //    0:d=0  hl=2 l=  25 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  10 prim: INTEGER           :FF0000000000000000
        //   14:d=1  hl=2 l=  11 prim: INTEGER           :FF000000000000000000
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/wAAAAAAAAAAAA==", "MBkCCgD/AAAAAAAAAAACCwD/AAAAAAAAAAAA")]
        // strip first, second normal
        // 00 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  67 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  31 prim: INTEGER           :22334455667788112233445566778811223344556677881122334455667788
        //   35:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("ACIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MEMCHyIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gCIBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneI")]
        // strip second, first normal
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 00 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  67 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //   36:d=1  hl=2 l=  31 prim: INTEGER           :22334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("ESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gAIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MEMCIBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIAh8iM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneI")]
        // strip both
        // 00 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 00 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  66 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  31 prim: INTEGER           :22334455667788112233445566778811223344556677881122334455667788
        //   35:d=1  hl=2 l=  31 prim: INTEGER           :22334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("ACIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gAIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MEICHyIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gCHyIzRFVmd4gRIjNEVWZ3iBEiM0RVZneIESIzRFVmd4g=")]
        // strip first to almost nothing
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  37 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   1 prim: INTEGER           :00
        //    5:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MCUCAQACIBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneI")]
        // strip first to just one byte
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 11
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  37 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   1 prim: INTEGER           :11
        //    5:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABERIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MCUCARECIBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iBEiM0RVZneI")]
        // strip first to just one byte, but with top bit set.
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F1
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  38 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   2 prim: INTEGER           :F1
        //    6:d=1  hl=2 l=  32 prim: INTEGER           :1122334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPERIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MCYCAgDxAiARIjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==")]
        // strip first to just one bit
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01
        // 00 00 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // 11 22 33 44 55 66 77 88 11 22 33 44 55 66 77 88
        // Expected output:
        //    0:d=0  hl=2 l=  35 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   1 prim: INTEGER           :01
        //    5:d=1  hl=2 l=  30 prim: INTEGER           :334455667788112233445566778811223344556677881122334455667788
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAADNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==", "MCMCAQECHjNEVWZ3iBEiM0RVZneIESIzRFVmd4gRIjNEVWZ3iA==")]
        // strip bot to just one bit
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01
        // Expected output:
        //    0:d=0  hl=2 l=   6 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   1 prim: INTEGER           :01
        //    5:d=1  hl=2 l=   1 prim: INTEGER           :01
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQ==", "MAYCAQECAQE=")]
        // strip both to one topbit.
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0
        // Expected output:
        //    0:d=0  hl=2 l=   8 cons: SEQUENCE          
        //    2:d=1  hl=2 l=   2 prim: INTEGER           :80
        //    6:d=1  hl=2 l=   2 prim: INTEGER           :F0
        //
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA8A==", "MAgCAgCAAgIA8A==")]
        // positive set and negative
        // 7f 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 84 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // Expected output:
        //    0:d=0  hl=2 l=  69 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  32 prim: INTEGER           :7F00000000000000000000000000000000000000000000000000000000000000
        //   36:d=1  hl=2 l=  33 prim: INTEGER           :8400000000000000000000000000000000000000000000000000000000000000
        //
        [InlineData("fwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==", "MEUCIH8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAiEAhAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
        // ... the bits of the first octet and bit 8 of the second octet:
        // 1. shall not all be ones and
        // 2. shall not all be zero.
        // FF F0 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // Expected output:
        //    0:d=0  hl=2 l=  69 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  33 prim: INTEGER           :FFF0000000000000000000000000000000000000000000000000000000000000
        //   37:d=1  hl=2 l=  32 prim: INTEGER           :0300000000000000000000000000000000000000000000000000000000000000
        //
        [InlineData("//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==", "MEUCIQD/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIgAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
        // ... the bits of the first octet and bit 8 of the second octet:
        // 1. shall not all be ones and
        // 2. shall not all be zero.
        // FF F0 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF
        // 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF
        // Expected output:
        //    0:d=0  hl=2 l=  69 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  33 prim: INTEGER           :FFF00000000000000000000000000000000000000000000000000000000000FF
        //   37:d=1  hl=2 l=  32 prim: INTEGER           :03000000000000000000000000000000000000000000000000000000000000FF
        //
        [InlineData("//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP8DAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/w==", "MEUCIQD/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/wIgAwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP8=")]
        // 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 7F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
        // Expected output:
        //    0:d=0  hl=2 l=  69 cons: SEQUENCE          
        //    2:d=1  hl=2 l=  33 prim: INTEGER           :8000000000000000000000000000000000000000000000000000000000000000
        //   37:d=1  hl=2 l=  32 prim: INTEGER           :7F00000000000000000000000000000000000000000000000000000000000000
        //
        [InlineData("gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==", "MEUCIQCAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIgfwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
        [Theory]
        public void Batch3(string input, string expected)
        {
            var inputBytes = Convert.FromBase64String(input);
            var expectedBytes = Convert.FromBase64String(expected);

            Trace.WriteLine($"Input:          {ToHex(inputBytes)}");
            Trace.WriteLine($"Expected: {ToHex(expectedBytes)}");
            Trace.WriteLine($"Output:   {ToHex(new X962PackagingFix().Format(inputBytes))}");

            Assert.Equal(expectedBytes, new X962PackagingFix().Format(inputBytes));
        }


        string ToHex(byte[] id)
        {
            var result = new StringBuilder(id.Length * 2);
            foreach (var i in id)
                result.AppendFormat("{0:x2}", i);

            return result.ToString();
        }
    }
}
