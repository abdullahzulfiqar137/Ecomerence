export const API_ENDPOINTS = {
  auth: {
    register: '/auth/register',
    login: '/auth/login',
    refreshToken: '/auth/refresh-token',
    revokeToken: '/auth/revoke-token',
    me: '/auth/me',
    forgotPassword: '/auth/forgot-password',
    resetPassword: '/auth/reset-password',
    changePassword: '/auth/change-password',
    verifyEmail: '/auth/verify-email'
  },
  products: {
    base: '/products',
    byId: (id: string) => `/products/${id}`,
    bySlug: (slug: string) => `/products/slug/${slug}`,
    featured: '/products/featured',
    variants: (productId: string) => `/products/${productId}/variants`,
    uploadImage: (productId: string) => `/products/${productId}/images`
  },
  categories: {
    base: '/categories',
    byId: (id: string) => `/categories/${id}`,
    bySlug: (slug: string) => `/categories/slug/${slug}`,
    featured: '/categories/featured'
  },
  brands: {
    base: '/brands',
    byId: (id: string) => `/brands/${id}`,
    bySlug: (slug: string) => `/brands/slug/${slug}`,
    featured: '/brands/featured'
  },
  cart: {
    base: '/cart',
    items: '/cart/items',
    itemById: (itemId: string) => `/cart/items/${itemId}`,
    merge: '/cart/merge'
  },
  wishlist: {
    base: '/wishlist',
    itemByProductId: (productId: string) => `/wishlist/items/${productId}`
  },
  checkout: {
    quote: '/checkout/quote',
    process: '/checkout/process'
  },
  orders: {
    base: '/orders',
    byId: (id: string) => `/orders/${id}`,
    byNumber: (orderNumber: string) => `/orders/number/${orderNumber}`,
    tracking: (trackingNumber: string) => `/orders/tracking/${trackingNumber}`,
    cancel: (id: string) => `/orders/${id}/cancel`
  },
  payments: {
    intent: '/payments/intent',
    byId: (id: string) => `/payments/${id}`,
    byOrderId: (orderId: string) => `/payments/order/${orderId}`
  },
  shipping: {
    rates: '/shipping/rates',
    methods: '/shipping/methods',
    zones: '/shipping/zones'
  },
  coupons: {
    validate: '/coupons/validate'
  },
  reviews: {
    byProduct: (productId: string) => `/reviews/product/${productId}`,
    create: '/reviews',
    upvote: (reviewId: string) => `/reviews/${reviewId}/upvote`
  },
  currencies: {
    base: '/currencies',
    active: '/currencies/active',
    convert: '/currencies/convert',
    countries: '/currencies/countries'
  },
  search: {
    base: '/search'
  },
  media: {
    upload: '/media/upload'
  },
  analytics: {
    dashboard: '/analytics/dashboard',
    salesTrend: '/analytics/sales-trend',
    topProducts: '/analytics/top-products',
    orderDistribution: '/analytics/order-status-distribution'
  },
  admin: {
    users: '/admin/users',
    userById: (id: string) => `/admin/users/${id}`,
    assignRoles: (id: string) => `/admin/users/${id}/roles`,
    updateStatus: (id: string) => `/admin/users/${id}/status`
  },
  diagnostics: {
    ping: '/diagnostics/ping',
    info: '/diagnostics/info'
  }
};
