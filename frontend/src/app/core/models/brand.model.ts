export interface Brand {
  id: string;
  name: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  website?: string;
  isActive: boolean;
  isFeatured: boolean;
  productCount: number;
}
