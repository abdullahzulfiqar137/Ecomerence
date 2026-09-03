import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { Category } from '../models/category.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryApiService {
  constructor(private readonly api: ApiService) {}

  getCategories(): Observable<Category[]> {
    return this.api.get<Category[]>(API_ENDPOINTS.categories.base).pipe(
      map(res => res.data || [])
    );
  }

  getFeaturedCategories(): Observable<Category[]> {
    return this.api.get<Category[]>(API_ENDPOINTS.categories.featured).pipe(
      map(res => res.data || [])
    );
  }

  getCategoryBySlug(slug: string): Observable<Category> {
    return this.api.get<Category>(API_ENDPOINTS.categories.bySlug(slug)).pipe(
      map(res => res.data)
    );
  }
}
