export interface Currency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  exchangeRateToBase: number;
  isBaseCurrency: boolean;
  isActive: boolean;
}

export interface Country {
  id: string;
  isoCode: string;
  name: string;
  phonePrefix: string;
  defaultCurrency: string;
  isActive: boolean;
}
