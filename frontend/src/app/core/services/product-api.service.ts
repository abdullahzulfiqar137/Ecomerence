import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ProductListItem, ProductDetails, ProductFilterParams } from '../models/product.model';
import { PagedResult } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ProductApiService {
  constructor(private readonly api: ApiService) {}

  getProducts(params?: ProductFilterParams): Observable<PagedResult<ProductListItem>> {
    return this.api.get<PagedResult<ProductListItem>>(API_ENDPOINTS.products.base, params).pipe(
      map(res => res.data)
    );
  }

  getFeaturedProducts(): Observable<ProductListItem[]> {
    return this.api.get<ProductListItem[]>(API_ENDPOINTS.products.featured).pipe(
      map(res => res.data || [])
    );
  }

  getProductById(id: string): Observable<ProductDetails> {
    return this.api.get<ProductDetails>(API_ENDPOINTS.products.byId(id)).pipe(
      map(res => res.data)
    );
  }

  getProductBySlug(slug: string): Observable<ProductDetails> {
    return this.api.get<ProductDetails>(API_ENDPOINTS.products.bySlug(slug)).pipe(
      map(res => res.data)
    );
  }
}
