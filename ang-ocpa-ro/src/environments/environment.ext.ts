import hmacSHA256 from 'crypto-js/hmac-sha256';
import { ext } from 'src/main';

export class Ext {
    seed=()=>ext.e();
    calc=(a,b,x)=>ext.d(this.hash(x,this.hash(a,b)),ext.c(),x);
    hash=(a,b)=>hmacSHA256(b,a).toString();
}
