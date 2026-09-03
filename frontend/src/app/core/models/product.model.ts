export interface ProductListItem {
  id: string;
  name: string;
  slug: string;
  sku: string;
  price: number;
  salePrice?: number;
  mainImageUrl?: string;
  categoryName?: string;
  brandName?: string;
  isInStock: boolean;
  averageRating: number;
  totalReviews: number;
  isFeatured: boolean;
  type: number;
}

export interface ProductDetails {
  id: string;
  name: string;
  slug: string;
  sku: string;
  shortDescription?: string;
  description?: string;
  price: number;
  salePrice?: number;
  costPrice?: number;
  categoryId: string;
  categoryName?: string;
  brandId?: string;
  brandName?: string;
  isInStock: boolean;
  stockQuantity: number;
  averageRating: number;
  totalReviews: number;
  isFeatured: boolean;
  images: ProductImage[];
  variants: ProductVariant[];
  attributes: Record<string, string>;
}

export interface ProductImage {
  id: string;
  imageUrl: string;
  altText?: string;
  isMain: boolean;
  displayOrder: number;
}

export interface ProductVariant {
  id: string;
  productId: string;
  sku: string;
  price: number;
  salePrice?: number;
  stockQuantity: number;
  isInStock: boolean;
  attributes: Record<string, string>;
}

export interface ProductFilterParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  isFeatured?: boolean;
  inStockOnly?: boolean;
  sortBy?: string;
}
