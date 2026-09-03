export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  displayOrder: number;
  isActive: boolean;
  isFeatured: boolean;
  children: Category[];
}
